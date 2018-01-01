# Grammar Primitives:  disjunction

Disjunction is based on [logical disjunction](https://en.wikipedia.org/wiki/Logical_disjunction).  It is a composite rule matching what any of its component would match.

For instance, the follow grammar:

```Python
rule main = "a" | "b";
```

will match either *a* or *b*.

Disjunctions must have at least two components but can have many.

```Python
rule main = "abc"* | "d" | "e"+ | "f";
```


---
[Go back to online documentation](../README.md)