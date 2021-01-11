# Object Output

We can output an object or an array containing constant, regardless of matching text of a rule.

Here are different examples:

Type|Grammar|Text|Output
-|-|-|-
Array|`rule main = "hi" => [1,2];`|"hi"|`[1,2]`
Array|`rule main = a:"a"* b:"b"* => [a,b];`|"aabbb"|`[ ["a", "a"],["b", "b", "b"] ]`
Array|`rule main = a:("a"* => text) b:("b"* => text) => [a,b];`|"aabbb"|`["aa", "bbb"]`
Object|`rule main = a:("a"* => text) b:("b"* => text) => { "myProperty":a, b:a };`|"aabbb"|`{ "myProperty": "aa", "bbb": "aa" }`

---
[Go back to online documentation](../README.md)