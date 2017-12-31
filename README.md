# Parser as a Service

See the [following blog article](http://todefine.com) introducing it.

Parser as a Service (PAS):  a grammar-based parser running as a REST service.  It allows you to implement
Domain Specific Languages (DSLs).

A workbench is [available online](https://pasworkbench.azurewebsites.net/).  It allows us to experiment
with the underlying API in an interactive manner.

Parser as a Service (PAS) is based on the defunct project 'M' from Microsoft, circa 2008.  The workbench takes inspiration from the three columns M editor.

[Documentation is available here](documentation/README.md).  For now, let's do a quick start.

## Implementing a simple DSL

Let's say we want to implement a simple DSL.  We want to be able to read a configuration file of the form:

```Python
location = Canada
sizes = large, medium
nodes = 5,2
nastier
=
elephant
,
prince
postal-code =
 K1P1J9
```

Let's say we can describe sample texts (in English) as follow:

* It is a list of configuration, arbitrarily long
* Each configuration is of the form ```config-key = config-element1, config-element2, ...``` where
  * ```config-key``` is an identifier starting with a letter and having any alpha-numeric character or minus ('-') afterwards
  * Config elements can be either alpha-numeric and minuses ('-') or they can be numeric ; they can't be both
  * Config elements can be either one element or multiple elements
* Configurations typically occupy different line but that isn't necessary the case

Let's try to implement this with PAS.  Let's fire up the [Workbench](https://pasworkbench.azurewebsites.net/).

![Starting state for the Workbench UI](documentation/images/Workbench-start.PNG "Workbench UI")

The Workbench UI has three columns:  the grammar, the sample text and the analysis.  We defined the grammar in the first one, we input some sample text in the second and the third one gets populated automatically by applying the grammar on the sample text.

Let's start simple by parsing words.  Let's input the following in the *Grammar* text area:

```Python
rule main = ("a".."z")*;
```

A grammar is a set of rules.  The *main* rule is the one that gets fire to parse text, but it can refer to other rules in more complex scenarios.

Here our grammar stipulate that anything between "a" and "z" is ok and can be repeated (arbitrarily long).  The '*' marks the repetition.

and let's type ```test``` in the *Sample Text* area.  The *Analysis* should display the following:

```JSON
200:  {
  "apiVersion": "0.0.92.72",
  "ruleMatch": {
    "rule": "main",
    "text": "test",
    "repeats": [
      {
        "text": "t"
      },
      {
        "text": "e"
      },
      {
        "text": "s"
      },
      {
        "text": "t"
      }
    ]
  }
}
```

The sample ```test``` does match our grammar:  it is a repetition of letters.

The **200** at the beginning of the analysis simply indicates the API HTTP status code.  **200** is HTTP for OK.

The ``ruleMatch`` element contains parsing information.  It is a hierarchical representation of the match that occured.  At the first level, we see the rule ``main`` was the one doing the match on the text ``test``.  We then see via the element ``repeats`` each sub rule that was fired.  The sub rule doesn't have a name so we only see the text it matched, i.e. each letter.  The main rule was ``("a".."z")*``.  The sub rule was ``"a".."z"``.

Let's keep going.  If we change the *Sample Text* for ``TEST``, we'll see the *Analysis* turn red with the following message:

```JSON
400:  Text cannot be matched by grammar
```

This is because our grammar recognizes only lower-case letters.  Let's fix that:

```Python
rule main = ("a".."z" | "A".."Z")+;
```
