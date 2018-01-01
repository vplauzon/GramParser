# Grammar Primitives:  literal

Literals are simple strings.

For instance, the follow grammar:

```Python
rule main = "bob";
```

will match one and only one sample text:  *bob*.

Literal is of course a fundamental primitive and is typically combined with other primitives.

## Escape characters

Literal allow for escaping characters:

|Escape sequence|Represents|
|---|---|
|\n|New Line|
|\r|Carriage Return|
|\t|Horizontal tab|
|\v|Vertical tab|
|\\\\|Backslash|
|\xhh|ASCII character in heradecimal notation|

---
[Go back to online documentation](../README.md)