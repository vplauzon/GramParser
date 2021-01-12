# Flatten Output function

We can flatten a list of arrays into a single array.

For instance, the following grammar:

```Python
rule main = .* => flatten([[1], [2,3], [4]]);
```

would match any text and output `[1,2,3,4]`.

This function is useful with optional match.  For instance, the following grammar:

```Python
rule identifier = ("a".."z")+ => text;
rule idList = head:identifier tail:("," id:identifier => id)* => prepend(head, tail);
rule bracketedIdList = "(" l:idList ")" => l;

rule main = functionName:identifier parameters:(bracketedIdList? => flatten(output));
```

would match the text "f(a,b)" with an output of `{
    "functionName": "f",
    "parameters": [
      "a",
      "b"
    ]
  }` and would match "f" with an output of `{
    "functionName": "f",
    "parameters": []
  }`.

---
[Go back to online documentation](../../README.md)