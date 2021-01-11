# Prepend Output function

We can prepend an array with another element.

Here are different examples:

Grammar|Text|Output
-|-|-|-
`rule main = a:"a"* "!" => prepend("my-prefix", a);`|"aa!"|`["my-prefix", "a", "a"]`

This function is useful when we implement a list-match with the head/tail pattern.  For instance, the following grammar:

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