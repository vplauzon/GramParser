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
rule idList = head:identifier tail:("," id:identifier => id)* => prepend(head, tail);

rule main = functionName:identifier (("(" l:idList ")" => l)? => flatten(output));
```

would match the text "f()" with an output of `[
    "a",
    "b",
    "c",
    "def"
  ]`.

---
[Go back to online documentation](../../README.md)