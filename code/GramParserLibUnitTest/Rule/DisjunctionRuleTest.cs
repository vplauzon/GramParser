using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using GramParserLib.Rule;
using System.Collections.Immutable;
using GramParserLib.Output;

namespace GramParserLibUnitTest.Rule
{
    [TestClass]
    public class DisjunctionRuleTest : BaseTest
    {
        [TestMethod]
        public void DisjunctionWithoutTags()
        {
            var samples = new[]
            {
                Tuple.Create("Alice", true),
                Tuple.Create("Bob", true),
                Tuple.Create("Charles", true),
                Tuple.Create("Didier", false)
            };
            //  Alice | Bob | Charles
            var rule = new DisjunctionRule("Disjunction", null, new[]
            {
                new TaggedRule(new LiteralRule("Alice", null, "Alice")),
                new TaggedRule(new LiteralRule("Bob", null, "Bob")),
                new TaggedRule(new LiteralRule("Charles", null, "Charles"))
            });

            for (int i = 0; i != samples.Length; ++i)
            {
                var text = samples[i].Item1;
                var isSuccess = samples[i].Item2;
                var match = rule.Match(new ExplorerContext(text)).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsNull(match, $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                    Assert.AreEqual(text, match.Text.ToString(), $"Content - {i}");
                }
            }
        }

        [TestMethod]
        public void DisjunctionWithTags()
        {
            var samples = new[]
            {
                ("Alice", true),
                ("Bob", true),
                ("Charles", true),
                ("Didier", false),
                ("Darwin", true),
                ("Ernest", true),
                ("Ephreme", false)
            };
            //  a:Alice | s1:(b:Bob | c:Charles) | s2::(d:Darwin | e:Ernest)
            var subRule1 = new DisjunctionRule("Disjunction", null, new[]
            {
                new TaggedRule("b", new LiteralRule("Bob", null, "Bob")),
                new TaggedRule("c", new LiteralRule("Charles", null, "Charles"))
            });
            var subRule2 = new DisjunctionRule("Disjunction", null, new[]
            {
                new TaggedRule("d", new LiteralRule("Darwin", null, "Darwin")),
                new TaggedRule("e", new LiteralRule("Ernest", null, "Ernest"))
            });
            var rule = new DisjunctionRule("Disjunction", null, new[]
            {
                new TaggedRule("a", new LiteralRule("Alice", null, "Alice")),
                new TaggedRule("s1", subRule1),
                new TaggedRule("s2", subRule2)
            });

            for (int i = 0; i != samples.Length; ++i)
            {
                (var text, var isSuccess) = samples[i];
                var match = rule.Match(new ExplorerContext(text)).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsNull(match, $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");

                    var output = ToMap(match.ComputeOutput());

                    Assert.AreEqual(1, output.Count, $"Fragments Count - {i}");
                    if (output.First().Key == "s1")
                    {
                        Assert.AreNotEqual(
                            0,
                            ToMap(output["s1"]).Count,
                            $"Sub Fragments 1 - {i}");
                    }
                    if (output.First().Key == "s2")
                    {
                        Assert.AreEqual(
                            1,
                            ToMap(output["s2"]).Count,
                            $"Sub Fragments 2 - {i}");
                    }
                }
            }
        }

        [TestMethod]
        public void DisjunctionWithRepeat()
        {
            var samples = new[]
            {
                Tuple.Create("ababab", true),
                Tuple.Create("aaaaaa", true),
                Tuple.Create("bbbbbbb", true),
                Tuple.Create("bbbbaaaabbbaaaa", true),
                Tuple.Create("", true),
                Tuple.Create("kaaaaabbbbb", false),
                Tuple.Create("kaaaa", false)
            };
            //  ('a'* | 'b'*)*
            var aRule = new RepeatRule("RepeatA", null, new LiteralRule("A", null, "a"), null, null);
            var bRule = new RepeatRule("RepeatB", null, new LiteralRule("B", null, "b"), null, null);
            var disjunction = new DisjunctionRule("Disjunction", null, new[]
            {
                new TaggedRule(aRule),
                new TaggedRule(bRule)
            });
            var rule = new RepeatRule("MasterRepeat", null, disjunction, null, null);

            for (int i = 0; i != samples.Length; ++i)
            {
                var text = samples[i].Item1;
                var isSuccess = samples[i].Item2;
                var match = rule.Match(new ExplorerContext(text)).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsTrue(
                        match == null || match.Text.Length != text.Length,
                        $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                }
            }
        }
   }
}