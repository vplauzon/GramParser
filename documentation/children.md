# Children Details

A recurring theme within PAS grammar is how to manage the details about the children of a rule-match.

By default, children details are returned.  Often this is more details than we need:  it takes up the bandwidth (when exchanging with the PAS API), memory to store it and compute to handle it.

Often we want to prune the children details.  But not all the time.  This section shows how to control this.

## Non-composite rules

Non-composite rules, i.e. rules that do not leverage other rules, do not match children.

Those rules are the *literal* rule (e.g. ``rule r = "Chair";``), the *any* rule (e.g. ``rule r = .;``) and the *range* rule (e.g. ``rule r = "a".."c";``).

For those rules, children details are never returned as they do not have children.  They are the leaves of the match tree.

## Using rule parameter

The following grammar:

```Python
rule main = "a"*;
```

applied to the text ``aaa`` will return three (3) sub matches (for each instance of the literal 'a').

We can force children details not to show up in a rule match using the *children* parameter:

```Python
rule(children=false) main = "a"*;
```

Similarly for the other composite rules, i.e. Disjunction, Sequence & Substraction rules:

```Python
rule a = "a"*;
rule b = "b"*;
rule disj = a | b;
rule seq = a b;
rule sub = a - "aa";
```

The last three rules would match by returning the details of rule *a* and / or *b*.

We could force the children details not to be returned as we did with the Repeat rule:

```Python
rule a = "a"*;
rule b = "b"*;
rule(children=false) disj = a | b;
rule(children=false) seq = a b;
rule(children=false) sub = a - "aa";
```

## Selecting children matches

We could also select which sub-rule we want to get details on.  This is only possible for Disjunction, Sequence & Substraction rules since the Repeat rule, by its nature, doesn't allow that.  Selection is done by prepending the rule with a colon.

```Python
rule a = "a"*;
rule b = "b"*;
rule disj = :a | b;
rule seq = a :b;
rule sub = :a - "aa";
```

In the example, for rule *disj* we chose to only have details on rule *a*, for rule *seq* only *b* and for *sub*, only *a*.

This is very useful as it is a concise way to specify which children we are interested in.  We typically omit constants that way.

## Ignoring grand-children

We might be interested in some children within a rule but not their children.  For instance, in the previous example, rule *seq* would display the details of rule *b*, i.e. a match for each literal.  We could forgo the children of that child, i.e. the grand-children of the main rule, by selecting the sub rule and prefixing it with two colons:

```Python
rule a = "a"*;
rule b = "b"*;
rule disj = ::a | b;
rule seq = a ::b;
rule sub = ::a - "aa";
```

Of course, if rule *a* already forced not to return its children details, having only one colon wouldn't bring back those grand-children.

## Naming children matches

Finally, we can name children match:

```Python
rule a = "a"*;
rule b = "b"*;
rule disj = a:a | b;
rule seq = a ::b;
rule sub = a::a - "aa";
```

Here, in rule *disj*, we choose to see the child match of rule *a* and we name that match *a*.  We proceeded similarly for *seq* and *sub* but this time we chose to ignore the grand children.

Naming the children match is useful to avoid to rely on the order of match.  This can make the client code more robust to changes in the grammar.

## Summary

We've seen different ways to control the level of details on a rule match.

Mastering this allows us to optimize the returned payload from the parsing API.