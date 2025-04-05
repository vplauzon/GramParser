# Merge Output function

We can merge different objects into a single object.

Here are different examples:

| Grammar | Text | Output 
|---------|------|--------
| `rule main = "1"* => merge({"a":text, "b":42}, {"c":true});` | "1" | `{"a":"1", "b":42, "c":true}` |


---
[Go back to online documentation](../../README.md)