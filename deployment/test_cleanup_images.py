import importlib.util
import json
import pathlib
import subprocess
import sys
import unittest
from unittest import mock


spec = importlib.util.spec_from_file_location(
    "cleanup_images", pathlib.Path(__file__).with_name("cleanup-images.py")
)
cleanup = importlib.util.module_from_spec(spec)
spec.loader.exec_module(cleanup)


def digest(number):
    return f"sha256:{number:064x}"


def item(number, day, children=()):
    return {
        "digest": digest(number),
        "createdTime": f"2026-01-{day:02d}T00:00:00Z",
        "references": [{"digest": digest(child)} for child in children],
    }


class CleanupTests(unittest.TestCase):
    def test_keeps_five_newest_and_deletes_old_images(self):
        retained, deleted = cleanup.plan_cleanup([item(i, i) for i in range(1, 9)], digest(8))
        self.assertEqual(retained, [digest(i) for i in range(8, 3, -1)])
        self.assertEqual(set(deleted), {digest(i) for i in range(1, 4)})

    def test_preserves_shared_children_and_deletes_parent_before_child(self):
        images = [item(1, 1, (20, 21)), item(2, 2, (20,))]
        images += [item(i, i) for i in range(3, 7)]
        images += [item(20, 1), item(21, 1)]
        retained, deleted = cleanup.plan_cleanup(images, digest(6))
        self.assertEqual(len(retained), 5)
        self.assertEqual(deleted, [digest(1), digest(21)])
        self.assertNotIn(digest(20), deleted)

    def test_refuses_to_delete_active_old_image(self):
        with self.assertRaisesRegex(ValueError, "five newest"):
            cleanup.plan_cleanup([item(i, i) for i in range(1, 7)], digest(1))

    def test_rejects_missing_child_and_cycles(self):
        with self.assertRaisesRegex(ValueError, "missing referenced"):
            cleanup.plan_cleanup([item(1, 1, (2,))], digest(1))
        with self.assertRaisesRegex(ValueError, "cyclic"):
            cleanup.plan_cleanup([item(1, 1), item(2, 2, (3,)), item(3, 3, (2,))], digest(1))

    def test_handles_fewer_than_five(self):
        retained, deleted = cleanup.plan_cleanup([item(1, 1), item(2, 2)], digest(2))
        self.assertEqual(len(retained), 2)
        self.assertEqual(deleted, [])

    def test_cli_deletes_only_old_manifests(self):
        manifest_list = json.dumps([item(i, i) for i in range(1, 8)])
        response = subprocess.CompletedProcess(args=[], returncode=0, stdout=manifest_list)
        with mock.patch.object(sys, "argv", ["cleanup-images.py", "registry", "workbench", digest(7)]):
            with mock.patch.object(cleanup.subprocess, "run", return_value=response) as run:
                cleanup.main()
        self.assertIn("[]", run.call_args_list[0].args[0])
        deleted = [call.args[0][call.args[0].index("--image") + 1] for call in run.call_args_list[1:]]
        self.assertEqual(set(deleted), {f"workbench@{digest(1)}", f"workbench@{digest(2)}"})

    def test_cli_does_not_delete_if_active_image_would_be_removed(self):
        manifest_list = json.dumps([item(i, i) for i in range(1, 8)])
        response = subprocess.CompletedProcess(args=[], returncode=0, stdout=manifest_list)
        with mock.patch.object(sys, "argv", ["cleanup-images.py", "registry", "workbench", digest(1)]):
            with mock.patch.object(cleanup.subprocess, "run", return_value=response) as run:
                with self.assertRaisesRegex(ValueError, "five newest"):
                    cleanup.main()
        self.assertEqual(run.call_count, 1)


if __name__ == "__main__":
    unittest.main()
