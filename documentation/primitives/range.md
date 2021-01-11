# Range rule

Range rule define a range of characters.  For instance, the following grammar:

```Python
rule main = "a".."c";
```

accepts *a*, *b* & *c*.

Ranges accepts the same escaping sequence as [literals](literal.md).  Similarly, characters can be single or double quoted in a range rule.

## Default Output

Range rule outputs the character it matches.  For instance, the following rule:

```Python
rule main = "a".."c";
```

will match `b` and output `"b"`.

---
[Go back to online documentation](../README.md)