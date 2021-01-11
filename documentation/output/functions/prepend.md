# Prepend Output function

We can prepend an array with another element.

Here are different examples:

Grammar|Text|Output
-|-|-|-
`rule main = a:"a"* "!" => prepend("my-prefix", a);`|"aa!"|`["my-prefix", "a", "a"]`

This function is useful when we implement a list-match with the head/tail pattern.  For instance, the following grammar:

```Python
rule identifier = ("a".."z")* => text;
rule list = head:identifier tail:("," id:identifier => id)* => prepend(head, tail);
rule parameters = "(" l:list ")" => l;

rule main = id:identifier p:parameters? => {"id":id, "params":flatten(p) };
```

would match the text "aaa(b,z)" with an output of `{
    "params": [
      "b",
      "z"
    ],
    "id": "aaa"
  }` and the text "aaa" with the output `{
    "params": [],
    "id": "aaa"
  }`.

---
[Go back to online documentation](../../README.md)