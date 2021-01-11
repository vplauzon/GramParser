# Constant Output

We can output a constant, regardless of matching text of a rule.

Here are different examples (sample text isn't included as it is irrelevant):

Type|Grammar|Output
-|-|-
Boolean|`rule main = .* => true;`|`true`
Boolean|`rule main = .* => false;`|`false`
Null|`rule main = .* => null;`|`null`
String|`rule main = .* => "abc";`|`"abc"`
Integer|`rule main = .* => 42;`|`42`
Integer|`rule main = .* => -72;`|`72`
Real|`rule main = .* => -42.42;`|`42.42`

