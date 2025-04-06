# Cast Output functions

We can cast an output into a specific type.

Here are different examples:

Type|Grammar|Text|Output
-|-|-|-
Boolean|`rule main = "true" | "false" => boolean(text);`|"true"|`true`
Boolean|`rule main = "true" | "false" => boolean(text);`|"false"|`false`
Integer|`rule main = ("0".."9")+ => integer(text);`|"42"|`42`
Float|`rule main = "-"? ("0".."9")* ("." ("0".."9")+)? => float(text);`|"42.5"|`42.6`

---
[Go back to online documentation](../../README.md)