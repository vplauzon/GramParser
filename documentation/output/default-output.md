# Default Output

We can access the default output of a rule.

This is useful to call a function needing that default output as a parameter.

For instance, the following grammar:

```Python
rule main = "42" => integer(output);
```

would match "42" text and output `42`.

---
[Go back to online documentation](../README.md)