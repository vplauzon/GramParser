# Flatten Output function

We can flatten a list of arrays into a single array.

For instance, the following grammar:

```Python
rule main = .* => flatten([[1], [2,3], [4]]);
```

would match any text and output `[1,2,3,4]`.

## Optional Match

This function is useful with optional match.  For instance, the following grammar:

```Python
rule identifier = ("a".."z")+ => text;
rule idList = head:identifier tail:("," id:identifier => id)* => prepend(head, tail);
rule bracketedIdList = "(" l:idList ")" => l;

rule main = functionName:identifier parameters:bracketedIdList? => flatten(output));
```

would have the following matches:

Text|Output
-|-
"f(a,b)"| `{ "functionName": "f", "parameters": [ "a", "b" ] }`
"f"|`{ "functionName": "f", "parameters": [] }`

An alternative would be to consider using the [firstOrNull function](first-or-null.md).  The difference is that function would yield a `null` instead of an empty list.

---
[Go back to online documentation](../../README.md)
