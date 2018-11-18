using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PasLib
{
    public static class MetaGrammar
    {
        #region Inner Types
        private class GrammarCreator
        {
            #region Inner Types
            private class PropertyBag
            {
                public static PropertyBag Default { get; } = new PropertyBag();

                public bool? HasInterleave { get; set; }

                public bool? IsRecursive { get; set; }

                public bool? HasChildrenDetails { get; set; }

                public bool IsDefault
                {
                    get
                    {
                        return !HasInterleave.HasValue
                            && !IsRecursive.HasValue
                            && !HasChildrenDetails.HasValue;
                    }
                }
            }
            #endregion

            private readonly Dictionary<string, IRule> _ruleMap =
                new Dictionary<string, IRule>();
            private readonly Dictionary<string, List<RuleProxy>> _proxies =
                new Dictionary<string, List<RuleProxy>>();

            public GrammarCreator(RuleMatch match)
            {
                IRule interleave = null;

                foreach (var ruleMatch in match.Children)
                {
                    var tag = ruleMatch.NamedChildren.Keys.First();
                    var subMatch = ruleMatch.NamedChildren.Values.First();

                    if (tag == "interleaveDeclaration")
                    {
                        var ruleBodyMatch = subMatch.NamedChildren.First().Value;

                        interleave = CreateRule(
                            "#interleave", PropertyBag.Default, ruleBodyMatch);
                    }
                    else if (tag == "ruleDeclaration")
                    {
                        var ruleID = subMatch.NamedChildren["id"].Text.ToString();
                        var parameterAssignationList = subMatch.NamedChildren["params"];
                        var ruleBody = subMatch.NamedChildren["body"];
                        var rule = CreateRule(ruleID, parameterAssignationList, ruleBody);

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
                var tagChild = tag.NamedChildren.First();

                switch (tagChild.Key)
                {
                    case "noTag":
                        return new TaggedRule(rule);
                    case "noChildrenTag":
                        {
                            var id = tagChild.Value.NamedChildren;

                            if (id.Count() == 0)
                            {
                                return new TaggedRule(null, rule, false);
                            }
                            else
                            {
                                var idText = id.First().Value.Text;

                                return new TaggedRule(
                                    idText.Length==0 ? null : idText.ToString(),
                                    rule,
                                    false);
                            }
                        }
                    case "withChildrenTag":
                        {
                            var id = tagChild.Value.NamedChildren;

                            if (id.Count() == 0)
                            {
                                return new TaggedRule(null, rule, true);
                            }
                            else
                            {
                                var idText = id.First().Value.Text;

                                return new TaggedRule(
                                    idText.Length==0 ? null : idText.ToString(),
                                    rule,
                                    true);
                            }
                        }
                    default:
                        throw new NotSupportedException(
                            $"Tag of type {tagChild.Key} isn't supported");
                }
            }

            private IRule CreateRule(RuleMatch ruleBodyMatch)
            {
                return CreateRule(null, PropertyBag.Default, ruleBodyMatch);
            }

            private IRule CreateRule(
                string ruleID,
                RuleMatch parameterAssignationList,
                RuleMatch ruleBodyMatch)
            {
                var propertyBag = CreatePropertyBag(parameterAssignationList);

                return CreateRule(ruleID, propertyBag, ruleBodyMatch);
            }

            private IRule CreateRule(
                string ruleID,
                PropertyBag propertyBag,
                RuleMatch ruleBodyMatch)
            {
                var ruleBodyTag = ruleBodyMatch.NamedChildren.First().Key;
                var ruleBodyBody = ruleBodyMatch.NamedChildren.First().Value;

                switch (ruleBodyTag)
                {
                    case "ruleRef":
                        return CreateRuleReference(ruleID, ruleBodyBody, propertyBag);
                    case "literal":
                        return CreateLiteral(ruleID, ruleBodyBody, propertyBag);
                    case "any":
                        return CreateAnyCharacter(ruleID, ruleBodyBody, propertyBag);
                    case "range":
                        return CreateRange(ruleID, ruleBodyBody, propertyBag);
                    case "repeat":
                        return CreateRepeat(ruleID, ruleBodyBody, propertyBag);
                    case "disjunction":
                        return CreateDisjunction(ruleID, ruleBodyBody, propertyBag);
                    case "sequence":
                        return CreateSequence(ruleID, ruleBodyBody, propertyBag);
                    case "substract":
                        return CreateSubstract(ruleID, ruleBodyBody, propertyBag);
                    case "bracket":
                        return CreateBracket(ruleID, ruleBodyBody, propertyBag);
                    default:
                        throw new NotSupportedException(ruleBodyTag);
                }
            }

            private PropertyBag CreatePropertyBag(RuleMatch parameterAssignationList)
            {
                if (parameterAssignationList == null
                    || parameterAssignationList.Children.Count == 0)
                {
                    return PropertyBag.Default;
                }
                else
                {
                    var list = parameterAssignationList.Children.First();
                    var head = list.NamedChildren["head"];
                    var tail = list.NamedChildren["tail"];
                    var tailParamAssignations = from t in tail.Children
                                                select t.NamedChildren.First().Value;
                    var paramAssignations = tailParamAssignations.Append(head);
                    var bag = new PropertyBag();

                    foreach (var assignation in paramAssignations)
                    {
                        var id = assignation.NamedChildren["id"].Text.ToString();
                        var value = assignation.NamedChildren["value"].Text.ToString();

                        switch (id)
                        {
                            case "interleave":
                                AssignInterleaveToBag(bag, value);
                                break;
                            case "recursive":
                                AssignRecursiveToBag(bag, value);
                                break;
                            case "children":
                                AssignChildrenToBag(bag, value);
                                break;
                            default:
                                throw new ParsingException(
                                    $"Parameter '{id}' isn't supported");
                        }
                    }

                    return bag;
                }
            }

            private static void AssignInterleaveToBag(PropertyBag bag, string value)
            {
                switch (value)
                {
                    case "true":
                        bag.HasInterleave = true;
                        break;
                    case "false":
                        bag.HasInterleave = false;
                        break;
                    default:
                        throw new ParsingException(
                            $"Value '{value}' isn't supported for interleave parameter");
                }
            }

            private static void AssignRecursiveToBag(PropertyBag bag, string value)
            {
                switch (value)
                {
                    case "true":
                        bag.IsRecursive = true;
                        break;
                    case "false":
                        bag.IsRecursive = false;
                        break;
                    default:
                        throw new ParsingException(
                            $"Value '{value}' isn't supported for recursive parameter");
                }
            }

            private static void AssignChildrenToBag(PropertyBag bag, string value)
            {
                switch (value)
                {
                    case "true":
                        bag.HasChildrenDetails = true;
                        break;
                    case "false":
                        bag.HasChildrenDetails = false;
                        break;
                    default:
                        throw new ParsingException(
                            $"Value '{value}' isn't supported for children parameter");
                }
            }

            private IRule CreateRuleReference(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag propertyBag)
            {
                if (!propertyBag.IsDefault)
                {
                    throw new ParsingException("Rule reference can't have parameters");
                }

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

            private IRule CreateLiteral(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag propertyBag)
            {
                var literal = ruleBodyBody.NamedChildren.First().Value;
                var characters = from l in literal.Children
                                 let c = GetCharacter(l)
                                 select c;
                var rule = new LiteralRule(ruleID, characters);

                return rule;
            }

            private char GetCharacter(RuleMatch character)
            {
                var charFragment = character.NamedChildren.First();
                var subMatch = charFragment.Value;

                switch (charFragment.Key)
                {
                    case "normal":
                        return subMatch.Text.First;
                    case "escapeLetter":
                        return GetEscapeLetter(subMatch.NamedChildren.First().Value.Text.First);
                    case "escapeQuote":
                        return '\"';
                    case "escapeBackslash":
                        return '\\';
                    case "escapeHexa":
                        return (char)int.Parse(
                            subMatch.NamedChildren.First().Value.Text.ToString(),
                            NumberStyles.HexNumber);
                    default:
                        throw new NotSupportedException(
                            $"Character tag not supported:  '{charFragment.Key}'");
                }
            }

            private char GetEscapeLetter(char letter)
            {
                switch (letter)
                {
                    case 'n':
                        return '\n';
                    case 'r':
                        return '\r';
                    case 't':
                        return '\t';
                    case 'v':
                        return '\v';
                    default:
                        throw new ParsingException(
                            $"Character escape not supported:  \\{letter}");
                }
            }

            private IRule CreateAnyCharacter(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag propertyBag)
            {
                return new MatchAnyCharacterRule(ruleID);
            }

            private IRule CreateRange(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag propertyBag)
            {
                var lower =
                            ruleBodyBody.NamedChildren["lower"].NamedChildren.First().Value;
                var upper =
                    ruleBodyBody.NamedChildren["upper"].NamedChildren.First().Value;
                var lowerChar = GetCharacter(lower);
                var upperChar = GetCharacter(upper);

                return new RangeRule(ruleID, lowerChar, upperChar);
            }

            private IRule CreateRepeat(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var subRuleBody = ruleBodyBody.NamedChildren["rule"];
                var rule = CreateRule(subRuleBody);
                var cardinality = ruleBodyBody.NamedChildren["cardinality"];

                switch (cardinality.NamedChildren.First().Key)
                {
                    case "star":
                        return new RepeatRule(
                            ruleID,
                            rule,
                            null,
                            null,
                            bag.HasInterleave,
                            bag.IsRecursive,
                            bag.HasChildrenDetails);
                    case "plus":
                        return new RepeatRule(
                            ruleID,
                            rule,
                            1,
                            null,
                            bag.HasInterleave,
                            bag.IsRecursive,
                            bag.HasChildrenDetails);
                    case "question":
                        return new RepeatRule(
                            ruleID,
                            rule,
                            0,
                            1,
                            bag.HasInterleave,
                            bag.IsRecursive,
                            bag.HasChildrenDetails);
                    case "exact":
                        {
                            var exact = cardinality.NamedChildren.First().Value;
                            var n = int.Parse(exact.NamedChildren.First().Value.Text.ToString());

                            return new RepeatRule(
                                ruleID,
                                rule,
                                n,
                                n,
                                bag.HasInterleave,
                                bag.IsRecursive,
                                bag.HasChildrenDetails);
                        }
                    case "minMax":
                        {
                            var minMaxCardinality = cardinality.NamedChildren.First().Value;
                            (var min, var max) = GetMinMaxCardinality(minMaxCardinality);

                            return new RepeatRule(
                                ruleID,
                                rule,
                                min,
                                max,
                                bag.HasInterleave,
                                bag.IsRecursive,
                                bag.HasChildrenDetails);
                        }
                    default:
                        throw new NotSupportedException();
                }
            }

            private (int? min, int? max) GetMinMaxCardinality(RuleMatch minMaxCardinality)
            {
                var type = minMaxCardinality.NamedChildren.First().Key;
                var cardinality = minMaxCardinality.NamedChildren.First().Value;

                switch (type)
                {
                    case "minmax":
                        {
                            var minText = cardinality.NamedChildren["min"].Text;
                            var maxText = cardinality.NamedChildren["max"].Text;
                            var min = int.Parse(minText.ToString());
                            var max = int.Parse(maxText.ToString());

                            return (min, max);
                        }
                    case "min":
                        {
                            var minText = cardinality.NamedChildren["min"].Text;
                            var min = int.Parse(minText.ToString());

                            return (min, null);
                        }
                    case "max":
                        {
                            var maxText = cardinality.NamedChildren["max"].Text;
                            var max = int.Parse(maxText.ToString());

                            return (null, max);
                        }
                    default:
                        throw new NotImplementedException($"Cardinality:  '{type}'");
                }
            }

            private IRule CreateDisjunction(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var headTag = ruleBodyBody.NamedChildren["t"];
                var head = ruleBodyBody.NamedChildren["head"];
                var tail = ruleBodyBody.NamedChildren["tail"];
                var headRule = CreateTaggedRule(headTag, CreateRule(head));
                var tailRules = from c in tail.Children
                                let tailTag = c.NamedChildren["t"]
                                let tailDisjunctable = c.NamedChildren["d"]
                                select CreateTaggedRule(
                                    tailTag, CreateRule(tailDisjunctable));
                var rules = new[] { headRule }.Concat(tailRules);

                return new DisjunctionRule(
                    ruleID, rules, bag.HasInterleave, bag.IsRecursive, bag.HasChildrenDetails);
            }

            private IRule CreateSequence(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var rules = from tagRule in ruleBodyBody.Children
                            let t = tagRule.NamedChildren["t"]
                            let r = tagRule.NamedChildren["r"]
                            let rule = CreateRule(r)
                            select CreateTaggedRule(t, rule);

                return new SequenceRule(
                    ruleID,
                    rules,
                    bag.HasInterleave,
                    bag.IsRecursive,
                    bag.HasChildrenDetails);
            }

            private IRule CreateSubstract(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var primary = ruleBodyBody.NamedChildren["primary"];
                var excluded = ruleBodyBody.NamedChildren["excluded"];
                var primaryRule = CreateRule(primary);
                var excludedRule = CreateRule(excluded);

                return new SubstractRule(
                    ruleID,
                    primaryRule,
                    excludedRule,
                    bag.HasInterleave,
                    bag.IsRecursive,
                    bag.HasChildrenDetails);
            }

            private IRule CreateBracket(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var bracketted = ruleBodyBody.NamedChildren.First().Value;
                var rule = CreateRule(null, bag, bracketted);

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
                new MatchAnyCharacterRule(null),
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
                false,
                false,
                false);
            var number = new RepeatRule(
                "identifier", new RangeRule(null, '0', '9'), 1, null, false, false, false);
            var character = GetCharacterRule();
            var quotedCharacter = new SequenceRule("quotedCharacter", new[]
            {
                new TaggedRule(new LiteralRule(null, "\"")),
                new TaggedRule("l", character, true),
                new TaggedRule(new LiteralRule(null, "\""))
            }, false, false);
            //  Rules
            var noChildrenTag = new SequenceRule("noChildrenTag", new[]
            {
                new TaggedRule(
                    "id",
                    new RepeatRule(null, identifier, 0, 1, false, false, false),
                    true),
                new TaggedRule(new LiteralRule(null, "::"))
            }, false, false);
            var withChildrenTag = new SequenceRule("withChildrenTag", new[]
            {
                new TaggedRule(
                    "id",
                    new RepeatRule(null, identifier, 0, 1, false, false, false),
                    true),
                new TaggedRule(new LiteralRule(null, ":"))
            }, false, false);
            var noTag = new LiteralRule("noTag", string.Empty);
            var tag = new DisjunctionRule("tag", new[]
            {
                new TaggedRule("noChildrenTag", noChildrenTag, true),
                new TaggedRule("withChildrenTag", withChildrenTag, true),
                new TaggedRule("noTag", noTag, true)
            }, false, false);
            var literal = new SequenceRule("literal", new[]
            {
                new TaggedRule(new LiteralRule(null, "\"")),
                new TaggedRule("l", new RepeatRule(null, character, null, null), true),
                new TaggedRule(new LiteralRule(null, "\""))
            }, false, false);
            var any = new LiteralRule("any", ".");
            var ruleBodyProxy = new RuleProxy();
            var range = new SequenceRule("range", new[]
            {
                new TaggedRule("lower", quotedCharacter, true),
                new TaggedRule(new RepeatRule(null, new LiteralRule(null, "."),2,2)),
                new TaggedRule("upper", quotedCharacter, true)
            }, false, false);
            var exactCardinality = new SequenceRule(
                "exactCardinality",
                new[]
                {
                    new TaggedRule(new LiteralRule(null, "{")),
                    new TaggedRule("n", number, true),
                    new TaggedRule(new LiteralRule(null, "}"))
                });
            var minMaxCardinality = new DisjunctionRule("minMaxCardinality", new[]
            {
                new TaggedRule("minmax", new SequenceRule("#minmax", new[]
                {
                    new TaggedRule(new LiteralRule(null, "{")),
                    new TaggedRule("min", number, true),
                    new TaggedRule(new LiteralRule(null, ",")),
                    new TaggedRule("max", number, true),
                    new TaggedRule(new LiteralRule(null, "}"))
                },
                isRecursive: false), true),
                new TaggedRule("min", new SequenceRule("#minmax", new[]
                {
                    new TaggedRule(new LiteralRule(null, "{")),
                    new TaggedRule("min", number, true),
                    new TaggedRule(new LiteralRule(null, ",")),
                    new TaggedRule(new LiteralRule(null, "}"))
                },
                isRecursive: false), true),
                new TaggedRule("max", new SequenceRule("#minmax", new[]
                {
                    new TaggedRule(new LiteralRule(null, "{")),
                    new TaggedRule(new LiteralRule(null, ",")),
                    new TaggedRule("max", number, true),
                    new TaggedRule(new LiteralRule(null, "}"))
                },
                isRecursive: false), true)
            });
            var cardinality = new DisjunctionRule("cardinality", new[]
            {
                new TaggedRule("star", new LiteralRule(null, "*"), true),
                new TaggedRule("plus", new LiteralRule(null, "+"), true),
                new TaggedRule("question", new LiteralRule(null, "?"), true),
                new TaggedRule("exact", exactCardinality, true),
                new TaggedRule("minMax", minMaxCardinality, true)
            },
            isRecursive: false);
            var bracket = new SequenceRule("bracket", new[]
            {
                new TaggedRule(new LiteralRule(null, "(")),
                new TaggedRule("r", ruleBodyProxy, true),
                new TaggedRule(new LiteralRule(null, ")"))
            },
            isRecursive: false);
            var repeatable = new DisjunctionRule("repeatable", new[]
            {
                new TaggedRule("ruleRef", identifier, true),
                new TaggedRule("literal", literal, true),
                new TaggedRule("bracket", bracket, true),
                new TaggedRule("any", any, true)
            },
            isRecursive: false);
            var repeat = new SequenceRule("repeat", new[]
            {
                new TaggedRule("rule", repeatable, true),
                new TaggedRule("cardinality", cardinality, true)
            },
            isRecursive: false);
            var disjunctionable = new DisjunctionRule("disjunctionable", new[]
            {
                new TaggedRule("ruleRef", identifier, true),
                new TaggedRule("literal", literal, true),
                new TaggedRule("range", range, true),
                new TaggedRule("bracket", bracket, true),
                new TaggedRule("any", any, true),
                new TaggedRule("repeat", repeat, true)
            },
            isRecursive: false);
            var disjunction = new SequenceRule("disjunction", new[]
            {
                new TaggedRule("t", tag, true),
                new TaggedRule("head", disjunctionable, true),
                new TaggedRule("tail", new RepeatRule(
                    null,
                    new SequenceRule(
                        null,
                        new[]
                        {
                            new TaggedRule(new LiteralRule(null, "|")),
                            new TaggedRule("t", tag, true),
                            new TaggedRule("d", disjunctionable, true)
                        }),
                    1,
                    null), true)
            },
            isRecursive: false);
            var sequenceable = new DisjunctionRule("sequenceable", new[]
            {
                new TaggedRule("ruleRef", identifier, true),
                new TaggedRule("literal", literal, true),
                new TaggedRule("range", range, true),
                new TaggedRule("bracket", bracket, true),
                new TaggedRule("any", any, true),
                new TaggedRule("repeat", repeat, true)
            },
            isRecursive: false);
            var innerSequence = new SequenceRule("innerSequence", new[]
            {
                new TaggedRule("t", tag, true),
                new TaggedRule("r", sequenceable, true)
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
                new TaggedRule("ruleRef", identifier, true),
                new TaggedRule("literal", literal, true),
                new TaggedRule("range", range, true),
                new TaggedRule("bracket", bracket, true),
                new TaggedRule("any", any, true),
                new TaggedRule("repeat", repeat, true)
            },
            isRecursive: false);
            var substracted = new DisjunctionRule("substracted", new[]
            {
                new TaggedRule("ruleRef", identifier, true),
                new TaggedRule("literal", literal, true),
                new TaggedRule("range", range, true),
                new TaggedRule("bracket", bracket, true),
                new TaggedRule("repeat", repeat, true)
            },
            isRecursive: false);
            var substract = new SequenceRule("substract", new[]
            {
                new TaggedRule("primary", substractable, true),
                new TaggedRule(new LiteralRule(null, "-")),
                new TaggedRule("excluded", substracted, true)
            },
            isRecursive: false);

            var ruleBody = new DisjunctionRule("ruleBody", new[]
            {
                new TaggedRule("ruleRef", identifier, true),
                new TaggedRule("literal", literal, true),
                new TaggedRule("range", range, true),
                new TaggedRule("bracket", bracket, true),
                new TaggedRule("any", any, true),
                new TaggedRule("substract", substract, true),
                new TaggedRule("disjunction", disjunction, true),
                new TaggedRule("repeat", repeat, true),
                new TaggedRule("sequence", sequence, true)
            },
            isRecursive: true);
            var interleaveDeclaration = new SequenceRule("interleaveDeclaration", new[]
            {
                new TaggedRule(new LiteralRule(null, "interleave")),
                new TaggedRule(new LiteralRule(null, "=")),
                new TaggedRule("body", ruleBodyProxy, true),
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
                new TaggedRule("id", identifier, true),
                new TaggedRule(new LiteralRule(null, "=")),
                new TaggedRule("value", boolean, true),
            },
            isRecursive: false);
            var innerParameterAssignationList = new SequenceRule(null, new[]
            {
                new TaggedRule(new LiteralRule(null, ",")),
                new TaggedRule("pa", parameterAssignation, true)
            });
            var parameterAssignationList = new SequenceRule("parameterAssignationList", new[]
            {
                new TaggedRule(new LiteralRule(null, "(")),
                new TaggedRule("head", parameterAssignation, true),
                new TaggedRule("tail", new RepeatRule(
                    null,
                    innerParameterAssignationList,
                    null,
                    null), true),
                new TaggedRule(new LiteralRule(null, ")"))
            },
            isRecursive: false);
            var ruleDeclaration = new SequenceRule("ruleDeclaration", new[]
            {
                new TaggedRule(new LiteralRule(null, "rule")),
                new TaggedRule(
                    "params",
                    new RepeatRule(null, parameterAssignationList, 0, 1),
                    true),
                new TaggedRule("id", identifier, true),
                new TaggedRule(new LiteralRule(null, "=")),
                new TaggedRule("body", ruleBody, true),
                new TaggedRule(new LiteralRule(null, ";"))
            },
            isRecursive: false);
            var declaration = new DisjunctionRule("declaration", new[]
            {
                new TaggedRule("interleaveDeclaration", interleaveDeclaration, true),
                new TaggedRule("ruleDeclaration", ruleDeclaration, true)
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
            var normal = new SubstractRule(
                null,
                new MatchAnyCharacterRule(null),
                new DisjunctionRule(null, TaggedRule.FromRules(new[]
                {
                    new LiteralRule(null, "\""),
                    new LiteralRule(null, "\r"),
                    new LiteralRule(null, "\n"),
                    new LiteralRule(null, "\\"),
                }), false, false),
                false,
                false);
            var escapeQuote = new LiteralRule(null, "\\\"");
            var escapeBackslash = new LiteralRule(null, "\\\\");
            var escapeLetter = new SequenceRule(null, new[]
            {
                new TaggedRule(new LiteralRule(null, "\\")),
                new TaggedRule("l", new DisjunctionRule(null, TaggedRule.FromRules(new[]
                {
                    new LiteralRule(null, "n"),
                    new LiteralRule(null, "r"),
                    new LiteralRule(null, "t"),
                    new LiteralRule(null, "v")
                })), true)
            }, false, false);
            var escapeHexa = new SequenceRule(null, new[]
            {
                new TaggedRule(new LiteralRule(null, "\\x")),
                new TaggedRule("h", new RepeatRule(
                    null,
                    new DisjunctionRule(null, TaggedRule.FromRules(new[]
                    {
                        new RangeRule(null, '0', '9'),
                        new RangeRule(null, 'a', 'f'),
                        new RangeRule(null, 'A', 'F')
                    }), false, false),
                    1,
                    2,
                    false,
                    false), false)
            }, false, false);
            var character = new DisjunctionRule("character", new[]
            {
                new TaggedRule("normal", normal, false),
                new TaggedRule("escapeQuote", escapeQuote, false),
                new TaggedRule("escapeBackslash", escapeBackslash, false),
                new TaggedRule("escapeLetter", escapeLetter, true),
                new TaggedRule("escapeHexa", escapeHexa, true)
            }, false, false);

            return character;
        }
    }
}