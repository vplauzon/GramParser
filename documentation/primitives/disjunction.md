# Disjunction rule

Disjunction is based on [logical disjunction](https://en.wikipedia.org/wiki/Logical_disjunction).  It is a composite rule matching what any of its component would match.

For instance, the follow grammar:

```Python
rule main = "a" | "b";
```

will match either *a* or *b*.

Disjunctions must have at least two components but can have more than two.

```Python
rule main = "abc"* | "d" | "e"+ | "f";
```

## Tagging

Either all sub rules must be tagged or none.

For instance, the first 2 rules are legal while the last one isn't:

```Python
rule rule1 = "a" | "b" | "c";
rule rule2 = a:"a" | b:"b" | c:"c";
# The first sub rule is tagged while the last two ones aren't:  illegal
rule rule3 = a:"a" | "b" | "c";
```

## Default Output

Disjunction rule outputs the output of the matching sub rule.

If tagging is used the output is a single-property object with the matching tag as property.

Here are different examples of possibilities:

Grammar|Text|Output
-|-|-
`rule main = "a" | "b"*;`|"a"|`"a"`
`rule main = "a" | "b"*;`|"bb"|`["b", "b"]`
`rule main = a:"a" | b:"b"*;`|"a"|`{"a":"a"}`
`rule main = a:"a" | b:"b"*;`|"bb"|`{"b": ["b", "b"]}`

---
[Go back to online documentation](../README.md)