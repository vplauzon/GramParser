# Grammar Primitives:  sequence

Sequence is a composite rule allowing to chain many rules.

For instance, the follow grammar:

```Python
rule main = "hi" "bob";
```

will match the sample text *hibob*.

A sequence must have at least two components but can have many.

```Python
rule main = "hi" ("a".."z")+ "bob" "5"*;
```

---
[Go back to online documentation](../README.md)