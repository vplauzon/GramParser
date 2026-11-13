#!/usr/bin/env python3
"""Keep the five newest root images, including their referenced manifests."""

import argparse
import datetime
import json
import re
import subprocess
import sys


DIGEST = re.compile(r"sha256:[0-9a-f]{64}\Z")
KEEP = 5


def plan_cleanup(manifests, deployed_digest):
    if not DIGEST.fullmatch(deployed_digest):
        raise ValueError("The deployed digest is invalid.")
    if not isinstance(manifests, list):
        raise ValueError("The registry did not return a manifest list.")

    nodes = {}
    for item in manifests:
        if not isinstance(item, dict):
            raise ValueError("Registry metadata contains an invalid manifest.")
        digest = item.get("digest")
        if not isinstance(digest, str) or not DIGEST.fullmatch(digest) or digest in nodes:
            raise ValueError("Registry metadata contains an invalid or duplicate digest.")
        created = item.get("createdTime")
        if not isinstance(created, str):
            raise ValueError(f"Missing creation time for {digest}.")
        timestamp = datetime.datetime.fromisoformat(created.replace("Z", "+00:00"))
        if timestamp.tzinfo is None:
            raise ValueError(f"Missing creation time zone for {digest}.")
        references = item.get("references") or []
        if not isinstance(references, list):
            raise ValueError(f"Invalid manifest references for {digest}.")
        children = []
        for reference in references:
            if not isinstance(reference, dict):
                raise ValueError(f"Invalid manifest reference in {digest}.")
            child = reference.get("digest")
            if not isinstance(child, str) or not DIGEST.fullmatch(child):
                raise ValueError(f"Invalid manifest reference in {digest}.")
            children.append(child)
        nodes[digest] = (timestamp, children)

    referenced = {child for _, children in nodes.values() for child in children}
    if referenced - nodes.keys():
        raise ValueError("Registry listing is missing referenced manifests; refusing cleanup.")
    roots = set(nodes) - referenced
    if deployed_digest not in roots:
        raise ValueError("The deployed digest is not a root image in the registry listing.")

    ordered_roots = sorted(roots, key=lambda digest: (nodes[digest][0], digest), reverse=True)
    retained = ordered_roots[:KEEP]
    if deployed_digest not in retained:
        raise ValueError("The deployed image is not among the five newest; refusing cleanup.")

    visited = set()
    visiting = set()
    postorder = []

    def visit(digest):
        if digest in visiting:
            raise ValueError("Registry metadata contains cyclic manifest references.")
        if digest in visited:
            return
        visiting.add(digest)
        for child in nodes[digest][1]:
            visit(child)
        visiting.remove(digest)
        visited.add(digest)
        postorder.append(digest)

    for digest in ordered_roots:
        visit(digest)
    if len(visited) != len(nodes):
        raise ValueError("Registry metadata contains unreachable or cyclic manifests.")

    protected = set()

    def protect(digest):
        if digest not in protected:
            protected.add(digest)
            for child in nodes[digest][1]:
                protect(child)

    for digest in retained:
        protect(digest)

    return retained, [digest for digest in reversed(postorder) if digest not in protected]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("registry")
    parser.add_argument("repository")
    parser.add_argument("deployed_digest")
    args = parser.parse_args()
    result = subprocess.run(
        ["az", "acr", "manifest", "list-metadata", "--registry", args.registry,
         "--name", args.repository, "--query", "[]", "--output", "json", "--only-show-errors"],
        check=True, capture_output=True, text=True,
    )
    retained, deleted = plan_cleanup(json.loads(result.stdout), args.deployed_digest)
    print(f"Keeping {len(retained)} newest root images in {args.repository}:", flush=True)
    for digest in retained:
        print(f"  {digest}", flush=True)
    for digest in deleted:
        print(f"Deleting {args.repository}@{digest}", flush=True)
        subprocess.run(
            ["az", "acr", "repository", "delete", "--name", args.registry,
             "--image", f"{args.repository}@{digest}", "--yes", "--only-show-errors"],
            check=True,
        )
    print(f"Deleted {len(deleted)} old manifests.", flush=True)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, subprocess.CalledProcessError) as error:
        if isinstance(error, subprocess.CalledProcessError) and error.stderr:
            print(error.stderr, file=sys.stderr, end="")
        print(f"Image cleanup failed: {error}", file=sys.stderr)
        sys.exit(1)
