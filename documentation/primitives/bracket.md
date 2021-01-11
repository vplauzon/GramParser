# Bracket rule

Backet rule groups rules together.

For instance, the following grammar:

```Python
rule main = ("a" | "b")*;
```

will match any sequence of *a* and *b*, e.g. "abbbaaaaba".

## Default Output

Bracket rule outputs the output of the rule it's embedding.  For instance, the following rule:

```Python
rule main = ("a" | "b")*;
```

would match "abba" and outputs `["a", "b", "b", "a"]`.  The array comes from the [repeat rule](repeat.md)'s output.

---
[Go back to online documentation](../README.md)