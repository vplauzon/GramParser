# Sequence rule

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

## Tagging

Tagging can be done on none of the sub rules, some or all.

## Default Output

### Without tags

If none of the sub rules are tagged, a sequence outputs an array.  For instance, the following rule:

```Python
rule main = "hi" "bob";
```

will parse "hibob" and outputs `["hi", "bob"]`.

### With tags

If some or all the sub rules are tagged, a sequence outputs an object with the tag as property name.  For instance, the following rule:

```Python
rule main = h:"hi" b:"bob";
```

will parse "hibob" and outputs `{"h": "hi", "b": "bob"]`.

---
[Go back to online documentation](../README.md)