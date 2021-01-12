# FirstOrNull Output function

We select the first element of an output list.  If the list doesn't contain any element, the output will be `null`.

For instance, the following grammar:

```Python
rule main = "a"* => firstOrNull(output);
```

would match "aaa" text and output `"a"` and would match "" and output `null`.

## Optional Match

This function is useful with optional match.  For instance, the following grammar:

```Python
rule identifier = ("a".."z")+ => text;

rule main = left:identifier sign:"="? right:identifier => 
{
  "left":left,
  "right":right,
  "sign":firstOrNull(sign)
};
```

would have the following matches:

Text|Output
-|-
"a=b"|`{ "sign": "=", "left": "a","right": "b" }`
"ab"|`{ "sign": null, "left": "a", "right": "b" }`.

---
[Go back to online documentation](../../README.md)