using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal static class MetaGrammar
    {
        private const string MAIN_RULE = "main";

        private static readonly RuleSet _metaSet = CreateMetaGrammar();

        public static RuleSet ParseGrammar(SubString text)
        {
            var result = _metaSet.Match(MAIN_RULE, text);

            if (result.IsFailure)
            {
                throw new ArgumentException("Problem parsing from:  " + result.Unmatched.ToString(), nameof(text));
            }
            else
            {
                var grammar = CreateGrammar(result.Match);

                return grammar;
            }
        }

        private static RuleSet CreateMetaGrammar()
        {
            //  Comments & interleaves
            var interleave = new DisjunctionRule("interleave", null, TaggedRule.FromRules(
                new LiteralRule(null, null, " "),
                new LiteralRule(null, null, "\r"),
                new LiteralRule(null, null, "\n"),
                new LiteralRule(null, null, "\t")));
            //  Tokens
            var identifierChar = new DisjunctionRule(null, null, TaggedRule.FromRules(
                new RangeRule(null, null, 'a', 'z'),
                new RangeRule(null, null, 'A', 'Z'),
                new RangeRule(null, null, '0', '9')));
            var identifier = new RepeatRule("identifier", null, identifierChar, 1, null, false, false);
            var number = new RepeatRule("identifier", null, new RangeRule(null, null, '0', '9'), 1, null, false, false);
            var character = GetCharacterRule();
            var quotedCharacter = new SequenceRule("literal", null, new[]
            {
                new TaggedRule(null, new LiteralRule(null, null, "\"")),
                new TaggedRule("l", character),
                new TaggedRule(null, new LiteralRule(null, null, "\""))
            });
            var literal = new SequenceRule("literal", null, new[]
            {
                new TaggedRule(null, new LiteralRule(null, null, "\"")),
                new TaggedRule("l", new RepeatRule(null, null, character, null, null, false, false)),
                new TaggedRule(null, new LiteralRule(null, null, "\""))
            });
            var any = new LiteralRule("any", null, ".");
            //  Rules
            var ruleBodyProxy = new RuleProxy();
            var range = new SequenceRule("range", interleave, new[]
            {
                new TaggedRule("lower", quotedCharacter),
                new TaggedRule(null, new RepeatRule(null, null, new LiteralRule(null, null, "."),2,2,false, false)),
                new TaggedRule("upper", quotedCharacter)
            });
            var exactCardinality = new SequenceRule("exactCardinality", interleave, new[]
            {
                new TaggedRule(null, new LiteralRule(null, null, "{")),
                new TaggedRule("n", number),
                new TaggedRule(null, new LiteralRule(null, null, "}"))
            });
            var minMaxCardinality = new SequenceRule("minMaxCardinality", interleave, new[]
            {
                new TaggedRule(null, new LiteralRule(null, null, "{")),
                new TaggedRule("min", number),
                new TaggedRule(null, new LiteralRule(null, null, ",")),
                new TaggedRule("max", number),
                new TaggedRule(null, new LiteralRule(null, null, "}"))
            });
            var cardinality = new DisjunctionRule("cardinality", interleave, new[]
            {
                new TaggedRule("star", new LiteralRule(null, null, "*")),
                new TaggedRule("plus", new LiteralRule(null, null, "+")),
                new TaggedRule("question", new LiteralRule(null, null, "?")),
                new TaggedRule("exact", exactCardinality),
                new TaggedRule("minMax", minMaxCardinality)
            });
            var repeat = new SequenceRule("repeat", interleave, new[]
            {
                new TaggedRule("rule", ruleBodyProxy),
                new TaggedRule("cardinality", cardinality)
            });
            var disjunction = new SequenceRule("disjunction", interleave, new[]
            {
                new TaggedRule("head", ruleBodyProxy),
                new TaggedRule("tail", new RepeatRule(
                    null,
                    interleave,
                    new SequenceRule(
                        null,
                        interleave,
                        new[]
                        {
                            new TaggedRule(null, new LiteralRule(null, null, "|")),
                            new TaggedRule("d", ruleBodyProxy)
                        }),
                    1,
                    null,
                    true,
                    true))
            });
            var sequence = new RepeatRule("sequence", interleave, ruleBodyProxy, 2, null, true, true);
            var substract = new SequenceRule("substract", interleave, new[]
            {
                new TaggedRule("primary", ruleBodyProxy),
                new TaggedRule(null, new LiteralRule(null, null, "-")),
                new TaggedRule("excluded", ruleBodyProxy)
            });
            var bracket = new SequenceRule("bracket", interleave, new[]
            {
                new TaggedRule(null, new LiteralRule(null, null, "(")),
                new TaggedRule("r", ruleBodyProxy),
                new TaggedRule(null, new LiteralRule(null, null, ")"))
            });

            var ruleBody = new DisjunctionRule("ruleBody", interleave, new[]
            {
                new TaggedRule("bracket", bracket),
                new TaggedRule("substract", substract),
                new TaggedRule("sequence", sequence),
                new TaggedRule("disjunction", disjunction),
                new TaggedRule("repeat", repeat),
                new TaggedRule("range", range),
                new TaggedRule("literal", literal),
                new TaggedRule("any", any)
            });
            var ruleDeclaration = new SequenceRule("ruleDeclaration", interleave, new[]
            {
                new TaggedRule(null, new LiteralRule(null, null, "rule")),
                new TaggedRule("id", identifier),
                new TaggedRule(null, new LiteralRule(null, null, "=")),
                new TaggedRule("body", ruleBody),
                new TaggedRule(null, new LiteralRule(null, null, ";"))
            });
            //  Main rule
            var main = new RepeatRule("main", interleave, ruleDeclaration, 1, null, true, true);

            ruleBodyProxy.ReferencedRule = ruleBody;

            return new RuleSet(interleave, new[] { main });
        }

        private static IRule GetCharacterRule()
        {
            var any = new MatchAnyCharacterRule(null, null);
            var noQuote = new SubstractRule(null, null, new TaggedRule(null, any), new LiteralRule(null, null, "\""));
            var noR = new SubstractRule(null, null, new TaggedRule(null, noQuote), new LiteralRule(null, null, "\r"));
            var noN = new SubstractRule(null, null, new TaggedRule(null, noR), new LiteralRule(null, null, "\n"));

            return noN;
        }

        private static RuleSet CreateGrammar(RuleMatch match)
        {
            var ruleMap = new Dictionary<string, IRule>();

            foreach (var ruleMatch in match.Contents)
            {
                var ruleID = ruleMatch.Fragments["id"].Content.ToString();
                var ruleBodyMatch = ruleMatch.Fragments["body"];
                var rule = CreateRule(ruleID, ruleBodyMatch);

                ruleMap[ruleID] = rule;
            }

            return new RuleSet(null, ruleMap.Values);
        }

        private static IRule CreateRule(string ruleID, RuleMatch ruleBodyMatch)
        {
            var ruleBodyTag = ruleBodyMatch.Fragments.Keys.First();
            var ruleBodyBody = ruleBodyMatch.Fragments.Values.First();

            switch (ruleBodyTag)
            {
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

        private static IRule CreateLiteral(string ruleID, RuleMatch ruleBodyBody)
        {
            var literal = ruleBodyBody.Fragments["l"].Content;
            var rule = new LiteralRule(ruleID, null, literal.Enumerate());

            return rule;
        }

        private static IRule CreateAnyCharacter(string ruleID, RuleMatch ruleBodyBody)
        {
            return new MatchAnyCharacterRule(ruleID, null);
        }

        private static IRule CreateRange(string ruleID, RuleMatch ruleBodyBody)
        {
            var lower = ruleBodyBody.Fragments["lower"].Fragments["l"].Content.First;
            var upper = ruleBodyBody.Fragments["upper"].Fragments["l"].Content.First;

            return new RangeRule(ruleID, null, lower, upper);
        }

        private static IRule CreateRepeat(string ruleID, RuleMatch ruleBodyBody)
        {
            var subRuleBody = ruleBodyBody.Fragments["rule"];
            var rule = CreateRule(null, subRuleBody);
            var cardinality = ruleBodyBody.Fragments["cardinality"];

            switch (cardinality.Fragments.Keys.First())
            {
                case "star":
                    return new RepeatRule(ruleID, null, rule, null, null, false, false);
                case "plus":
                    return new RepeatRule(ruleID, null, rule, 1, null, false, false);
                case "question":
                    return new RepeatRule(ruleID, null, rule, 0, 1, false, false);
                case "exact":
                    {
                        var exact = cardinality.Fragments.Values.First();
                        var n = int.Parse(exact.Fragments["n"].Content.ToString());

                        return new RepeatRule(ruleID, null, rule, n, n, false, false);
                    }
                case "minMax":
                    {
                        var minMax = cardinality.Fragments.Values.First();
                        var min = int.Parse(minMax.Fragments["min"].Content.ToString());
                        var max = int.Parse(minMax.Fragments["max"].Content.ToString());

                        return new RepeatRule(ruleID, null, rule, min, max, false, false);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private static IRule CreateDisjunction(string ruleID, RuleMatch ruleBodyBody)
        {
            var head = ruleBodyBody.Fragments["head"];
            var tail = ruleBodyBody.Fragments["tail"];
            var headRule = CreateRule(null, head);
            var tailRules = from c in tail.Contents
                            select CreateRule(null, c.Fragments["d"]);
            var rules = new[] { headRule }.Concat(tailRules);

            return new DisjunctionRule(ruleID, null, TaggedRule.FromRules(rules));
        }

        private static IRule CreateSequence(string ruleID, RuleMatch ruleBodyBody)
        {
            var rules = from r in ruleBodyBody.Contents
                        select CreateRule(null, r);

            return new SequenceRule(ruleID, null, TaggedRule.FromRules(rules));
        }

        private static IRule CreateSubstract(string ruleID, RuleMatch ruleBodyBody)
        {
            var primary = ruleBodyBody.Fragments["primary"];
            var excluded = ruleBodyBody.Fragments["excluded"];
            var primaryRule = CreateRule(null, primary);
            var excludedRule = CreateRule(null, excluded);

            return new SubstractRule(ruleID, null, new TaggedRule(null, primaryRule), excludedRule);
        }

        private static IRule CreateBracket(string ruleID, RuleMatch ruleBodyBody)
        {
            var bracketted = ruleBodyBody.Fragments["r"];
            var rule = CreateRule(null, bracketted);

            return rule;
        }
    }
}