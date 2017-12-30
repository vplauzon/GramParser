using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    public static class MetaGrammar
    {
        #region Inner Types
        private class GrammarCreator
        {
            private readonly Dictionary<string, IRule> _ruleMap =
                new Dictionary<string, IRule>();
            private readonly Dictionary<string, List<RuleProxy>> _proxies =
                new Dictionary<string, List<RuleProxy>>();

            public GrammarCreator(RuleMatch match)
            {
                IRule interleave = null;

                foreach (var ruleMatch in match.Repeats)
                {
                    var tag = ruleMatch.Fragments.Keys.First();

                    if (tag == "interleaveDeclaration")
                    {
                        var subMatch = ruleMatch.Fragments.First().Value;
                        var ruleBodyMatch = subMatch.Fragments["body"];

                        interleave = CreateRule("#interleave", ruleBodyMatch);
                    }
                    else if (tag == "ruleDeclaration")
                    {
                        var subMatch = ruleMatch.Fragments.First().Value;
                        var ruleID = subMatch.Fragments["id"].Text.ToString();
                        var parameterAssignationList = subMatch.Fragments["params"];
                        var ruleBody = subMatch.Fragments["body"];
                        var rule = CreateRule(ruleID, ruleBody);

                        if (parameterAssignationList.Repeats.Count != 0)
                        {
                            throw new NotImplementedException();
                        }

                        _ruleMap[ruleID] = rule;
                    }
                    else
                    {
                        throw new NotSupportedException($"Tag '{tag}' for declaration");
                    }
                }

                ResolveProxies();
                CreatedGrammar = new Grammar(_ruleMap, interleave);
            }

            private void ResolveProxies()
            {
                foreach (var pair in _proxies)
                {
                    var ruleName = pair.Key;
                    var referenceList = pair.Value;

                    if (!_ruleMap.ContainsKey(ruleName))
                    {
                        throw new ParsingException(
                            $"Can't find definition of rule '{ruleName}'");
                    }

                    var rule = _ruleMap[ruleName];

                    foreach (var proxy in referenceList)
                    {
                        proxy.ReferencedRule = rule;
                    }
                }
            }

            public Grammar CreatedGrammar { get; }

            private TaggedRule CreateTaggedRule(RuleMatch tag, IRule rule)
            {
                var fragment = tag.Fragments.First();

                switch (fragment.Key)
                {
                    case "noTag":
                        return new TaggedRule(rule);
                    case "withChildrenTag":
                        {
                            var id = fragment.Value.Fragments.First().Value.Text.ToString();

                            return new TaggedRule(id, rule, true);
                        }
                    case "noChildrenTag":
                        {
                            var id = fragment.Value.Fragments.First().Value.Text.ToString();

                            return new TaggedRule(id, rule, false);
                        }
                    default:
                        throw new NotSupportedException(
                            $"Tag of type {fragment.Key} isn't supported");
                }
            }

            private IRule CreateRule(string ruleID, RuleMatch ruleBodyMatch)
            {
                var ruleBodyTag = ruleBodyMatch.Fragments.Keys.First();
                var ruleBodyBody = ruleBodyMatch.Fragments.Values.First();

                switch (ruleBodyTag)
                {
                    case "ruleRef":
                        return CreateRuleReference(ruleID, ruleBodyBody);
                    case "literal":
                        return CreateLiteral(ruleID, ruleBodyBody);
                    case "any":
                        return CreateAnyCharacter(ruleID, ruleBodyBody);
                    case "range":
                        return CreateRange(ruleID, ruleBodyBody);
                    case "repeat":
                        return CreateRepeat(ruleID, ruleBodyBody);
                    case "disjunction":
                        return CreateDisjunction(ruleID, ruleBodyBody);
                    case "sequence":
                        return CreateSequence(ruleID, ruleBodyBody);
                    case "substract":
                        return CreateSubstract(ruleID, ruleBodyBody);
                    case "bracket":
                        return CreateBracket(ruleID, ruleBodyBody);
                    default:
                        throw new NotSupportedException(ruleBodyTag);
                }
            }

            private IRule CreateRuleReference(string ruleID, RuleMatch ruleBodyBody)
            {
                var identifier = ruleBodyBody.Text.ToString();

                //  If the referenced rule has already been parsed we insert it
                //  Otherwise, we put a proxy
                if (_ruleMap.ContainsKey(identifier))
                {
                    var rule = _ruleMap[identifier];

                    return rule;
                }
                else
                {
                    var rule = new RuleProxy();

                    if (!_proxies.ContainsKey(identifier))
                    {
                        _proxies[identifier] = new List<RuleProxy>();
                    }

                    var referenceList = _proxies[identifier];

                    //  Store the proxy for later resolving it
                    referenceList.Add(rule);

                    return rule;
                }
            }

            private IRule CreateLiteral(string ruleID, RuleMatch ruleBodyBody)
            {
                var literal = ruleBodyBody.Fragments["l"].Text;
                var rule = new LiteralRule(ruleID, literal.Enumerate());

                return rule;
            }

            private IRule CreateAnyCharacter(string ruleID, RuleMatch ruleBodyBody)
            {
                return new MatchAnyCharacterRule(ruleID);
            }

            private IRule CreateRange(string ruleID, RuleMatch ruleBodyBody)
            {
                var lower = ruleBodyBody.Fragments["lower"].Fragments["l"].Text.First;
                var upper = ruleBodyBody.Fragments["upper"].Fragments["l"].Text.First;

                return new RangeRule(ruleID, lower, upper);
            }

            private IRule CreateRepeat(string ruleID, RuleMatch ruleBodyBody)
            {
                var subRuleBody = ruleBodyBody.Fragments["rule"];
                var rule = CreateRule(null, subRuleBody);
                var cardinality = ruleBodyBody.Fragments["cardinality"];

                switch (cardinality.Fragments.Keys.First())
                {
                    case "star":
                        return new RepeatRule(ruleID, rule, null, null);
                    case "plus":
                        return new RepeatRule(ruleID, rule, 1, null);
                    case "question":
                        return new RepeatRule(ruleID, rule, 0, 1);
                    case "exact":
                        {
                            var exact = cardinality.Fragments.Values.First();
                            var n = int.Parse(exact.Fragments["n"].Text.ToString());

                            return new RepeatRule(ruleID, rule, n, n);
                        }
                    case "minMax":
                        {
                            var minMax = cardinality.Fragments.Values.First();
                            var min = int.Parse(minMax.Fragments["min"].Text.ToString());
                            var max = int.Parse(minMax.Fragments["max"].Text.ToString());

                            return new RepeatRule(ruleID, rule, min, max);
                        }
                    default:
                        throw new NotSupportedException();
                }
            }

            private IRule CreateDisjunction(string ruleID, RuleMatch ruleBodyBody)
            {
                var headTag = ruleBodyBody.Fragments["t"];
                var head = ruleBodyBody.Fragments["head"];
                var tail = ruleBodyBody.Fragments["tail"];
                var headRule = CreateTaggedRule(headTag, CreateRule(null, head));
                var tailRules = from c in tail.Repeats
                                let tailTag = c.Fragments["t"]
                                let tailDisjunctable = c.Fragments["d"]
                                select CreateTaggedRule(
                                    tailTag, CreateRule(null, tailDisjunctable));
                var rules = new[] { headRule }.Concat(tailRules);

                return new DisjunctionRule(ruleID, rules);
            }

            private IRule CreateSequence(string ruleID, RuleMatch ruleBodyBody)
            {
                var rules = from tagRule in ruleBodyBody.Repeats
                            let t = tagRule.Fragments["t"]
                            let r = tagRule.Fragments["r"]
                            let rule = CreateRule(null, r)
                            select CreateTaggedRule(t, rule);

                return new SequenceRule(ruleID, rules);
            }

            private IRule CreateSubstract(string ruleID, RuleMatch ruleBodyBody)
            {
                var primary = ruleBodyBody.Fragments["primary"];
                var excluded = ruleBodyBody.Fragments["excluded"];
                var tag = ruleBodyBody.Fragments["t"];
                var primaryRule = CreateTaggedRule(tag, CreateRule(null, primary));
                var excludedRule = CreateRule(null, excluded);

                return new SubstractRule(ruleID, primaryRule, excludedRule);
            }

            private IRule CreateBracket(string ruleID, RuleMatch ruleBodyBody)
            {
                var bracketted = ruleBodyBody.Fragments["r"];
                var rule = CreateRule(null, bracketted);

                return rule;
            }
        }
        #endregion

        private const string MAIN_RULE = "main";

        private static readonly Grammar _metaSet = CreateMetaGrammar();

        public static Grammar ParseGrammar(SubString text)
        {
            var match = _metaSet.Match(MAIN_RULE, text);

            if (match != null)
            {
                var grammarCreator = new GrammarCreator(match);
                var grammar = grammarCreator.CreatedGrammar;

                return grammar;
            }
            else
            {
                return null;
            }
        }

        private static Grammar CreateMetaGrammar()
        {
            //  Comments & interleaves
            var carriageReturn = new DisjunctionRule("#carriageReturn", TaggedRule.FromRules(
                new LiteralRule(null, "\r"),
                new LiteralRule(null, "\n")), false, false);
            var commentContentChar = new SubstractRule("#commentContentChar",
                new TaggedRule(new MatchAnyCharacterRule(null)),
                carriageReturn,
                false);
            var commentContent =
                new RepeatRule(null, commentContentChar, null, null, false, false);
            var comment = new SequenceRule("comment", TaggedRule.FromRules(
                new LiteralRule(null, "#"),
                commentContent), false, false);
            var interleave = new DisjunctionRule("$interleave$", TaggedRule.FromRules(
                new LiteralRule(null, " "),
                new LiteralRule(null, "\r"),
                new LiteralRule(null, "\n"),
                new LiteralRule(null, "\t"),
                comment), false, false);
            //  Tokens
            var identifierChar = new DisjunctionRule(null, TaggedRule.FromRules(
                new RangeRule(null, 'a', 'z'),
                new RangeRule(null, 'A', 'Z'),
                new RangeRule(null, '0', '9')), false, false);
            var identifier = new RepeatRule(
                "identifier",
                identifierChar,
                1,
                null,
                false);
            var number = new RepeatRule(
                "identifier", new RangeRule(null, '0', '9'), 1, null, false);
            var character = GetCharacterRule();
            var quotedCharacter = new SequenceRule("quotedCharacter", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "\"")),
                new TaggedRule("l", character),
                new TaggedRule(null, new LiteralRule(null, "\""))
            }, false, false);
            var literal = new SequenceRule("literal", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "\"")),
                new TaggedRule("l", new RepeatRule(null, character, null, null)),
                new TaggedRule(null, new LiteralRule(null, "\""))
            }, false, false);
            var any = new LiteralRule("any", ".");
            //  Rules
            var noChildrenTag = new SequenceRule("noChildrenTag", new[]
            {
                new TaggedRule("id", identifier),
                new TaggedRule(new LiteralRule(null, "::"))
            }, false, false);
            var withChildrenTag = new SequenceRule("withChildrenTag", new[]
            {
                new TaggedRule("id", identifier),
                new TaggedRule(new LiteralRule(null, ":"))
            }, false, false);
            var noTag = new LiteralRule("noTag", string.Empty);
            var tag = new DisjunctionRule("tag", new[]
            {
                new TaggedRule("noChildrenTag", noChildrenTag),
                new TaggedRule("withChildrenTag", withChildrenTag),
                new TaggedRule("noTag", noTag)
            }, false, false);
            var ruleBodyProxy = new RuleProxy();
            var range = new SequenceRule("range", new[]
            {
                new TaggedRule("lower", quotedCharacter),
                new TaggedRule(null, new RepeatRule(null, new LiteralRule(null, "."),2,2)),
                new TaggedRule("upper", quotedCharacter)
            }, false, false);
            var exactCardinality = new SequenceRule(
                "exactCardinality",
                new[]
                {
                    new TaggedRule(null, new LiteralRule(null, "{")),
                    new TaggedRule("n", number),
                    new TaggedRule(null, new LiteralRule(null, "}"))
                });
            var minMaxCardinality = new SequenceRule("minMaxCardinality", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "{")),
                new TaggedRule("min", number),
                new TaggedRule(null, new LiteralRule(null, ",")),
                new TaggedRule("max", number),
                new TaggedRule(null, new LiteralRule(null, "}"))
            },
            isRecursive: false);
            var cardinality = new DisjunctionRule("cardinality", new[]
            {
                new TaggedRule("star", new LiteralRule(null, "*")),
                new TaggedRule("plus", new LiteralRule(null, "+")),
                new TaggedRule("question", new LiteralRule(null, "?")),
                new TaggedRule("exact", exactCardinality),
                new TaggedRule("minMax", minMaxCardinality)
            },
            isRecursive: false);
            var bracket = new SequenceRule("bracket", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "(")),
                new TaggedRule("r", ruleBodyProxy),
                new TaggedRule(null, new LiteralRule(null, ")"))
            },
            isRecursive: false);
            var repeatable = new DisjunctionRule("repeatable", new[]
            {
                new TaggedRule("ruleRef", identifier),
                new TaggedRule("literal", literal),
                new TaggedRule("bracket", bracket),
                new TaggedRule("any", any)
            },
            isRecursive: false);
            var repeat = new SequenceRule("repeat", new[]
            {
                new TaggedRule("rule", repeatable),
                new TaggedRule("cardinality", cardinality)
            },
            isRecursive: false);
            var disjunctionable = new DisjunctionRule("disjunctionable", new[]
            {
                new TaggedRule("ruleRef", identifier),
                new TaggedRule("literal", literal),
                new TaggedRule("range", range),
                new TaggedRule("bracket", bracket),
                new TaggedRule("any", any),
                new TaggedRule("repeat", repeat)
            },
            isRecursive: false);
            var disjunction = new SequenceRule("disjunction", new[]
            {
                new TaggedRule("t", tag),
                new TaggedRule("head", disjunctionable),
                new TaggedRule("tail", new RepeatRule(
                    null,
                    new SequenceRule(
                        null,
                        new[]
                        {
                            new TaggedRule(null, new LiteralRule(null, "|")),
                            new TaggedRule("t", tag),
                            new TaggedRule("d", disjunctionable)
                        }),
                    1,
                    null))
            },
            isRecursive: false);
            var sequenceable = new DisjunctionRule("sequenceable", new[]
            {
                new TaggedRule("ruleRef", identifier),
                new TaggedRule("literal", literal),
                new TaggedRule("range", range),
                new TaggedRule("bracket", bracket),
                new TaggedRule("any", any),
                new TaggedRule("repeat", repeat)
            },
            isRecursive: false);
            var innerSequence = new SequenceRule("innerSequence", new[]
            {
                new TaggedRule("t", tag),
                new TaggedRule("r", sequenceable)
            },
            isRecursive: false);
            var sequence = new RepeatRule(
                "sequence",
                innerSequence,
                2,
                null,
                isRecursive: false);
            var substractable = new DisjunctionRule("substractable", new[]
            {
                new TaggedRule("ruleRef", identifier),
                new TaggedRule("literal", literal),
                new TaggedRule("range", range),
                new TaggedRule("bracket", bracket),
                new TaggedRule("any", any),
                new TaggedRule("repeat", repeat)
            },
            isRecursive: false);
            var substracted = new DisjunctionRule("substracted", new[]
            {
                new TaggedRule("ruleRef", identifier),
                new TaggedRule("literal", literal),
                new TaggedRule("range", range),
                new TaggedRule("bracket", bracket),
                new TaggedRule("repeat", repeat)
            },
            isRecursive: false);
            var substract = new SequenceRule("substract", new[]
            {
                new TaggedRule("t", tag),
                new TaggedRule("primary", substractable),
                new TaggedRule(null, new LiteralRule(null, "-")),
                new TaggedRule("excluded", substracted)
            },
            isRecursive: false);

            var ruleBody = new DisjunctionRule("ruleBody", new[]
            {
                new TaggedRule("ruleRef", identifier),
                new TaggedRule("literal", literal),
                new TaggedRule("range", range),
                new TaggedRule("bracket", bracket),
                new TaggedRule("any", any),
                new TaggedRule("substract", substract),
                new TaggedRule("disjunction", disjunction),
                new TaggedRule("repeat", repeat),
                new TaggedRule("sequence", sequence)
            },
            isRecursive: true);
            var interleaveDeclaration = new SequenceRule("interleaveDeclaration", new[]
            {
                new TaggedRule(new LiteralRule(null, "interleave")),
                new TaggedRule(new LiteralRule(null, "=")),
                new TaggedRule("body", ruleBodyProxy),
                new TaggedRule(new LiteralRule(null, ";"))
            },
            isRecursive: false);
            var boolean = new DisjunctionRule("boolean", new[]
            {
                new TaggedRule(new LiteralRule("true", "true")),
                new TaggedRule(new LiteralRule("false", "false"))
            },
            isRecursive: false);
            var parameterAssignation = new SequenceRule("parameterAssignation", new[]
            {
                new TaggedRule("id", identifier),
                new TaggedRule(new LiteralRule(null, "=")),
                new TaggedRule("value", boolean),
            },
            isRecursive: false);
            var innerParameterAssignationList = new SequenceRule(null, new[]
            {
                new TaggedRule(new LiteralRule(null, ",")),
                new TaggedRule(parameterAssignation)
            });
            var parameterAssignationList = new SequenceRule("parameterAssignationList", new[]
            {
                new TaggedRule(new LiteralRule(null, "(")),
                new TaggedRule("head", parameterAssignation),
                new TaggedRule("tail", new RepeatRule(
                    null,
                    innerParameterAssignationList,
                    null,
                    null)),
                new TaggedRule(new LiteralRule(null, ")"))
            },
            isRecursive: false);
            var ruleDeclaration = new SequenceRule("ruleDeclaration", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "rule")),
                new TaggedRule("params", new RepeatRule(null, parameterAssignationList, 0, 1)),
                new TaggedRule("id", identifier),
                new TaggedRule(null, new LiteralRule(null, "=")),
                new TaggedRule("body", ruleBody),
                new TaggedRule(null, new LiteralRule(null, ";"))
            },
            isRecursive: false);
            var declaration = new DisjunctionRule("declaration", new[]
            {
                new TaggedRule("interleaveDeclaration", interleaveDeclaration),
                new TaggedRule("ruleDeclaration", ruleDeclaration)
            },
            isRecursive: false);
            //  Main rule
            var main = new RepeatRule("main", declaration, 1, null, isRecursive: false);

            ruleBodyProxy.ReferencedRule = ruleBody;

            return new Grammar(
                new Dictionary<string, IRule>() { { main.RuleName, main } },
                interleave);
        }

        private static IRule GetCharacterRule()
        {
            var any = new MatchAnyCharacterRule(null);
            var noQuote = new SubstractRule(
                null,
                new TaggedRule(null, any),
                new LiteralRule(null, "\""),
                false);
            var noR = new SubstractRule(
                null,
                new TaggedRule(null, noQuote),
                new LiteralRule(null, "\r"),
                false);
            var noN = new SubstractRule(
                null,
                new TaggedRule(null, noR),
                new LiteralRule(null, "\n"),
                false);

            return noN;
        }
    }
}