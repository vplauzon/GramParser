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
            var match = _metaSet.Match(MAIN_RULE, text);

            if (match != null)
            {
                var grammar = CreateGrammar(match);

                return grammar;
            }
            else
            {
                return null;
            }
        }

        private static RuleSet CreateMetaGrammar()
        {
            //  Comments & interleaves
            var interleave = new DisjunctionRule("$interleave$", TaggedRule.FromRules(
                new LiteralRule(null, " "),
                new LiteralRule(null, "\r"),
                new LiteralRule(null, "\n"),
                new LiteralRule(null, "\t")));
            //  Tokens
            var identifierChar = new DisjunctionRule(null, TaggedRule.FromRules(
                new RangeRule(null, 'a', 'z'),
                new RangeRule(null, 'A', 'Z'),
                new RangeRule(null, '0', '9')));
            var identifier = new RepeatRule("identifier", identifierChar, 1, null);
            var number = new RepeatRule(
                "identifier", new RangeRule(null, '0', '9'), 1, null);
            var character = GetCharacterRule();
            var quotedCharacter = new SequenceRule("literal", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "\"")),
                new TaggedRule("l", character),
                new TaggedRule(null, new LiteralRule(null, "\""))
            });
            var literal = new SequenceRule("literal", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "\"")),
                new TaggedRule("l", new RepeatRule(null, character, null, null)),
                new TaggedRule(null, new LiteralRule(null, "\""))
            });
            var any = new LiteralRule("any", ".");
            //  Rules
            var ruleBodyProxy = new RuleProxy();
            var range = new SequenceRule("range", new[]
            {
                new TaggedRule("lower", quotedCharacter),
                new TaggedRule(null, new RepeatRule(null, new LiteralRule(null, "."),2,2)),
                new TaggedRule("upper", quotedCharacter)
            });
            var exactCardinality = new SequenceRule("exactCardinality", new[]
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
            });
            var cardinality = new DisjunctionRule("cardinality", new[]
            {
                new TaggedRule("star", new LiteralRule(null, "*")),
                new TaggedRule("plus", new LiteralRule(null, "+")),
                new TaggedRule("question", new LiteralRule(null, "?")),
                new TaggedRule("exact", exactCardinality),
                new TaggedRule("minMax", minMaxCardinality)
            });
            var repeat = new SequenceRule("repeat", new[]
            {
                new TaggedRule("rule", ruleBodyProxy),
                new TaggedRule("cardinality", cardinality)
            });
            var disjunction = new SequenceRule("disjunction", new[]
            {
                new TaggedRule("head", ruleBodyProxy),
                new TaggedRule("tail", new RepeatRule(
                    null,
                    new SequenceRule(
                        null,
                        new[]
                        {
                            new TaggedRule(null, new LiteralRule(null, "|")),
                            new TaggedRule("d", ruleBodyProxy)
                        }),
                    1,
                    null))
            });
            var sequence = new RepeatRule("sequence", ruleBodyProxy, 2, null);
            var substract = new SequenceRule("substract", new[]
            {
                new TaggedRule("primary", ruleBodyProxy),
                new TaggedRule(null, new LiteralRule(null, "-")),
                new TaggedRule("excluded", ruleBodyProxy)
            });
            var bracket = new SequenceRule("bracket", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "(")),
                new TaggedRule("r", ruleBodyProxy),
                new TaggedRule(null, new LiteralRule(null, ")"))
            });

            var ruleBody = new DisjunctionRule("ruleBody", new[]
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
            var ruleDeclaration = new SequenceRule("ruleDeclaration", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "rule")),
                new TaggedRule("id", identifier),
                new TaggedRule(null, new LiteralRule(null, "=")),
                new TaggedRule("body", ruleBody),
                new TaggedRule(null, new LiteralRule(null, ";"))
            });
            //  Main rule
            var main = new RepeatRule("main", ruleDeclaration, 1, null);

            ruleBodyProxy.ReferencedRule = ruleBody;

            return new RuleSet(new[] { main });
        }

        private static IRule GetCharacterRule()
        {
            var any = new MatchAnyCharacterRule(null);
            var noQuote = new SubstractRule(null, new TaggedRule(null, any), new LiteralRule(null, "\""));
            var noR = new SubstractRule(null, new TaggedRule(null, noQuote), new LiteralRule(null, "\r"));
            var noN = new SubstractRule(null, new TaggedRule(null, noR), new LiteralRule(null, "\n"));

            return noN;
        }

        private static RuleSet CreateGrammar(RuleMatch match)
        {
            var ruleMap = new Dictionary<string, IRule>();

            foreach (var ruleMatch in match.Repeats)
            {
                var ruleID = ruleMatch.Fragments["id"].Text.ToString();
                var ruleBodyMatch = ruleMatch.Fragments["body"];
                var rule = CreateRule(ruleID, ruleBodyMatch);

                ruleMap[ruleID] = rule;
            }

            return new RuleSet(ruleMap.Values);
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
            var literal = ruleBodyBody.Fragments["l"].Text;
            var rule = new LiteralRule(ruleID, literal.Enumerate());

            return rule;
        }

        private static IRule CreateAnyCharacter(string ruleID, RuleMatch ruleBodyBody)
        {
            return new MatchAnyCharacterRule(ruleID);
        }

        private static IRule CreateRange(string ruleID, RuleMatch ruleBodyBody)
        {
            var lower = ruleBodyBody.Fragments["lower"].Fragments["l"].Text.First;
            var upper = ruleBodyBody.Fragments["upper"].Fragments["l"].Text.First;

            return new RangeRule(ruleID, lower, upper);
        }

        private static IRule CreateRepeat(string ruleID, RuleMatch ruleBodyBody)
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

        private static IRule CreateDisjunction(string ruleID, RuleMatch ruleBodyBody)
        {
            var head = ruleBodyBody.Fragments["head"];
            var tail = ruleBodyBody.Fragments["tail"];
            var headRule = CreateRule(null, head);
            var tailRules = from c in tail.Repeats
                            select CreateRule(null, c.Fragments["d"]);
            var rules = new[] { headRule }.Concat(tailRules);

            return new DisjunctionRule(ruleID, TaggedRule.FromRules(rules));
        }

        private static IRule CreateSequence(string ruleID, RuleMatch ruleBodyBody)
        {
            var rules = from r in ruleBodyBody.Repeats
                        select CreateRule(null, r);

            return new SequenceRule(ruleID, TaggedRule.FromRules(rules));
        }

        private static IRule CreateSubstract(string ruleID, RuleMatch ruleBodyBody)
        {
            var primary = ruleBodyBody.Fragments["primary"];
            var excluded = ruleBodyBody.Fragments["excluded"];
            var primaryRule = CreateRule(null, primary);
            var excludedRule = CreateRule(null, excluded);

            return new SubstractRule(ruleID, new TaggedRule(null, primaryRule), excludedRule);
        }

        private static IRule CreateBracket(string ruleID, RuleMatch ruleBodyBody)
        {
            var bracketted = ruleBodyBody.Fragments["r"];
            var rule = CreateRule(null, bracketted);

            return rule;
        }
    }
}