# Concat Output functions

We can concat different strings together.

Here are different examples:

Grammar|Text|Output
-|-|-|-
`rule main = r1:("a" | "b") "=" r2:("c" | "d") => concat(r1, r2);`|"a=c"|`"ac"`
`rule main = r1:("a" | "b") "=" r2:("c" | "d") => concat("my string:  ", r1, r2, "...  here it ends");`|"a=c"|`"my string:  ac...  here it ends"`

---
[Go back to online documentation](../../README.md)