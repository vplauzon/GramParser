# Output text

We can output the entire text of a rule by simply using the `text` keyword.  For instance, the following rule:

```Python
rule main = "a"+ ("b" | "c")+ ("3".."6")+ => text;
```

will mach "aaabcbcbc345" and output the "aaabcbcbc345" as opposed to the default output of `[ [
      "a",
      "a",
      "a"
    ],
    [
      "b",
      "c",
      "b",
      "c",
      "b",
      "c"
    ],
    [
      "3",
      "4",
      "5"
    ]
  ]`.