# Flatten Output function

We can flatten a list of arrays into a single array.

For instance, the following grammar:

```Python
rule main = .* => flatten([[1], [2,3], [4]]);
```

would match any text and output `[1,2,3,4]`.

This function is useful with optional match.  For instance, the following grammar:

```Python
rule identifier = ("a".."z")* => text;

rule main = head:identifier tail:("," id:identifier => id)* => prepend(head, tail);
```

would match the text "a,b,c,def" with an output of `[
    "a",
    "b",
    "c",
    "def"
  ]`.

---
[Go back to online documentation](../../README.md)