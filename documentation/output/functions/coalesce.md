# Coalesce Output function

We return the first element of a list that is non-`null`.  If the list is empty or if no such non-`null` element is found, `null` is returned.

For instance, the following grammar:

```Python
rule main = "a"+ r:("=" => true)? "b"+ => coalesce([coalesce(r), false]);
```

would match "a=b" text and output `true` and would match "ab" and output `false`.

---
[Go back to online documentation](../../README.md)