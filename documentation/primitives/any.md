# Any rule

Any rule accepts all characters.

For instance, the follow grammar:

```Python
rule main = .;
```

will match any one-character sample text.

Any are typically used with a [substract](substract.md) rule in order to define a set of characters by excepting few.  For instance, the following grammar:

```Python
rule main = . - "a";
```

will accept any one-character sample text, except the letter *a*.

## Default Output

Any rule outputs the character it matches.  For instance, the following rule:

```Python
rule main = .;
```

will match `a` and output `"a"`.

---

[Go back to online documentation](../README.md)