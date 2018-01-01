# Grammar Primitives:  substract

Substract is a composite rule allowing to include the match of a rule while excluding the ones of another rule.

For instance, the follow grammar:

```Python
rule main = ("a" | "b" | "c") - "b";
```

will match the sample text *a* and *c*.  It is equivalent to the following grammar:

```Python
rule main = "a" | "c";
```

Substract are often used with any.  For instance, the following grammar:

```Python
rule main = .* - "fail";
```

will match any sample text except *fail*.

The substract rule can only have two components:  the primary and the excluded rules.  In order to exclude more than one rule we can either use recursivity, like here:

```Python
rule main = (.* - "fail") - "failure";
```

or we can substract from a disjunction:

```Python
rule main = .* - ("fail" | "failure");
```

The latter form is **more efficient** than the recursion to evaluate so **we recommend using it**.

---
[Go back to online documentation](../README.md)