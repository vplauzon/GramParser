﻿using System;
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

                public bool IsDefault
                {
                    get
                    {
                        return !HasInterleave.HasValue
                            && !IsRecursive.HasValue;
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

                foreach (var ruleMatch in match.Repeats)
                {
                    var taggedMatch = ruleMatch.Fragments.First();
                    var subMatch = taggedMatch.Match;

                    if (taggedMatch.Tag == "interleaveDeclaration")
                    {
                        var ruleBodyMatch = subMatch.Fragments.First().Match;

                        interleave = CreateRule(
                            "#interleave", PropertyBag.Default, ruleBodyMatch);
                    }
                    else if (taggedMatch.Tag == "ruleDeclaration")
                    {
                        var ruleID = subMatch.GetFragments("id").Text.ToString();
                        var parameterAssignationList = subMatch.GetFragments("params");
                        var ruleBody = subMatch.GetFragments("body");
                        var rule = CreateRule(ruleID, parameterAssignationList, ruleBody);

                        _ruleMap[ruleID] = rule;
                    }
                    else
                    {
                        throw new NotSupportedException($"Tag '{taggedMatch}' for declaration");
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

                switch (fragment.Tag)
                {
                    case "noTag":
                        return new TaggedRule(rule);
                    case "withChildrenTag":
                        {
                            var id = fragment.Match.Fragments.First().Match.Text.ToString();

                            return new TaggedRule(id, rule, true);
                        }
                    case "noChildrenTag":
                        {
                            var id = fragment.Match.Fragments.First().Match.Text.ToString();

                            return new TaggedRule(id, rule, false);
                        }
                    default:
                        throw new NotSupportedException(
                            $"Tag of type {fragment.Tag} isn't supported");
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
                var ruleBodyTag = ruleBodyMatch.Fragments.First().Tag;
                var ruleBodyBody = ruleBodyMatch.Fragments.First().Match;

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
                    || parameterAssignationList.Repeats.Count == 0)
                {
                    return PropertyBag.Default;
                }
                else
                {
                    var list = parameterAssignationList.Repeats.First();
                    var head = list.Fragments[0].Match;
                    var tail = list.Fragments[1].Match;
                    var tailParamAssignations = from t in tail.Repeats
                                                select t.Fragments.First().Match;
                    var paramAssignations = tailParamAssignations.Append(head);
                    var bag = new PropertyBag();

                    foreach (var assignation in paramAssignations)
                    {
                        var id = assignation.Fragments[0].Match.Text.ToString();
                        var value = assignation.Fragments[1].Match.Text.ToString();

                        switch (id)
                        {
                            case "interleave":
                                AssignInterleaveToBag(bag, value);
                                break;
                            case "recursive":
                                AssignRecursiveToBag(bag, value);
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

            private IRule CreateRuleReference(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag propertyBag)
            {
                if (!propertyBag.IsDefault)
                {
                    throw new NotSupportedException("Rule reference can't have parameters");
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
                if (!propertyBag.IsDefault)
                {
                    throw new NotSupportedException("Literal rule can't have parameters");
                }

                var literal = ruleBodyBody.Fragments.First().Match;
                var characters = from l in literal.Repeats
                                 let c = GetCharacter(l)
                                 select c;
                var rule = new LiteralRule(ruleID, characters);

                return rule;
            }

            private char GetCharacter(RuleMatch character)
            {
                var charFragment = character.Fragments.First();
                var subMatch = charFragment.Match;

                switch (charFragment.Tag)
                {
                    case "normal":
                        return subMatch.Text.First;
                    case "escapeLetter":
                        return GetEscapeLetter(subMatch.Fragments.First().Match.Text.First);
                    case "escapeQuote":
                        return '\"';
                    case "escapeBackslash":
                        return '\\';
                    case "escapeHexa":
                        return (char)int.Parse(
                            subMatch.Fragments.First().Match.Text.ToString(),
                            NumberStyles.HexNumber);
                    default:
                        throw new NotSupportedException(
                            $"Character tag not supported:  '{charFragment.Tag}'");
                }
            }

            private char GetEscapeLetter(char letter)
            {
                switch (letter)
                {
                    case 'a':
                        return '\a';
                    case 'b':
                        return '\b';
                    case 'f':
                        return '\f';
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
                if (!propertyBag.IsDefault)
                {
                    throw new NotSupportedException("Any character rule can't have parameters");
                }

                return new MatchAnyCharacterRule(ruleID);
            }

            private IRule CreateRange(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag propertyBag)
            {
                if (!propertyBag.IsDefault)
                {
                    throw new NotSupportedException("Range rule can't have parameters");
                }

                var lower =
                            ruleBodyBody.GetFragments("lower").Fragments.First().Match;
                var upper =
                    ruleBodyBody.GetFragments("upper").Fragments.First().Match;
                var lowerChar = GetCharacter(lower);
                var upperChar = GetCharacter(upper);

                return new RangeRule(ruleID, lowerChar, upperChar);
            }

            private IRule CreateRepeat(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var subRuleBody = ruleBodyBody.GetFragments("rule");
                var rule = CreateRule(subRuleBody);
                var cardinality = ruleBodyBody.GetFragments("cardinality");

                switch (cardinality.Fragments.First().Tag)
                {
                    case "star":
                        return new RepeatRule(
                            ruleID, rule, null, null, bag.HasInterleave, bag.IsRecursive);
                    case "plus":
                        return new RepeatRule(
                            ruleID, rule, 1, null, bag.HasInterleave, bag.IsRecursive);
                    case "question":
                        return new RepeatRule(
                            ruleID, rule, 0, 1, bag.HasInterleave, bag.IsRecursive);
                    case "exact":
                        {
                            var exact = cardinality.Fragments.First().Match;
                            var n = int.Parse(exact.Fragments.First().Match.Text.ToString());

                            return new RepeatRule(
                                ruleID, rule, n, n, bag.HasInterleave, bag.IsRecursive);
                        }
                    case "minMax":
                        {
                            var minMax = cardinality.Fragments.First().Match;
                            var min = int.Parse(minMax.GetFragments("min").Text.ToString());
                            var max = int.Parse(minMax.GetFragments("max").Text.ToString());

                            return new RepeatRule(
                                ruleID, rule, min, max, bag.HasInterleave, bag.IsRecursive);
                        }
                    default:
                        throw new NotSupportedException();
                }
            }

            private IRule CreateDisjunction(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var headTag = ruleBodyBody.GetFragments("t");
                var head = ruleBodyBody.GetFragments("head");
                var tail = ruleBodyBody.GetFragments("tail");
                var headRule = CreateTaggedRule(headTag, CreateRule(head));
                var tailRules = from c in tail.Repeats
                                let tailTag = c.GetFragments("t")
                                let tailDisjunctable = c.GetFragments("d")
                                select CreateTaggedRule(
                                    tailTag, CreateRule(tailDisjunctable));
                var rules = new[] { headRule }.Concat(tailRules);

                return new DisjunctionRule(ruleID, rules, bag.HasInterleave, bag.IsRecursive);
            }

            private IRule CreateSequence(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var rules = from tagRule in ruleBodyBody.Repeats
                            let t = tagRule.GetFragments("t")
                            let r = tagRule.GetFragments("r")
                            let rule = CreateRule(r)
                            select CreateTaggedRule(t, rule);

                return new SequenceRule(ruleID, rules, bag.HasInterleave, bag.IsRecursive);
            }

            private IRule CreateSubstract(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var primary = ruleBodyBody.GetFragments("primary");
                var excluded = ruleBodyBody.GetFragments("excluded");
                var tag = ruleBodyBody.GetFragments("t");
                var primaryRule = CreateTaggedRule(tag, CreateRule(primary));
                var excludedRule = CreateRule(excluded);

                return new SubstractRule(
                    ruleID, primaryRule, excludedRule, bag.HasInterleave, bag.IsRecursive);
            }

            private IRule CreateBracket(
                string ruleID,
                RuleMatch ruleBodyBody,
                PropertyBag bag)
            {
                var bracketted = ruleBodyBody.Fragments.First().Match;
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
                new TaggedRule("l", character, true),
                new TaggedRule(null, new LiteralRule(null, "\""))
            }, false, false);
            var literal = new SequenceRule("literal", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "\"")),
                new TaggedRule("l", new RepeatRule(null, character, null, null), true),
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
                new TaggedRule("pa", parameterAssignation)
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
            var noBackSlash = new SubstractRule(
                null,
                new TaggedRule(null, noN),
                new LiteralRule(null, "\\"),
                false);
            var escapeQuote = new LiteralRule(null, "\\\"");
            var escapeBackslash = new LiteralRule(null, "\\\\");
            var escapeLetter = new SequenceRule(null, new[]
            {
                new TaggedRule(new LiteralRule(null, "\\")),
                new TaggedRule("l", new DisjunctionRule(null, TaggedRule.FromRules(new[]
                {
                    new LiteralRule(null, "a"),
                    new LiteralRule(null, "b"),
                    new LiteralRule(null, "f"),
                    new LiteralRule(null, "n"),
                    new LiteralRule(null, "r"),
                    new LiteralRule(null, "t"),
                    new LiteralRule(null, "v")
                })))
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
                    false))
            }, false, false);
            var character = new DisjunctionRule("character", new[]
            {
                new TaggedRule("normal", noBackSlash, true),
                new TaggedRule("escapeQuote", escapeQuote, true),
                new TaggedRule("escapeBackslash", escapeBackslash, true),
                new TaggedRule("escapeLetter", escapeLetter, true),
                new TaggedRule("escapeHexa", escapeHexa, true)
            }, false, false);

            return character;
        }
    }
}