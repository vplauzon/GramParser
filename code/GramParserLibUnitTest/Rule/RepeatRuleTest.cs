using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using GramParserLib.Rule;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Rule
{
    [TestClass]
    public class RepeatRuleTest : BaseTest
    {
        [TestMethod]
        public void EmptyRepeat()
        {
            var oneCharRule = new LiteralRule("oneChar", null, "g");
            var rule = new RepeatRule("Repeat", null, oneCharRule, null, null) as IRule;
            var match = rule.Match(new ExplorerContext("")).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Rule");
            Assert.AreEqual(0, match.Text.Length, "MatchLength");
            Assert.AreEqual(0, ToList(match.ComputeOutput()).Count(), "Contents");
        }

        [TestMethod]
        public void RepeatOneCharacter()
        {
            var oneCharRule = new LiteralRule("oneChar", null, "g");
            var rule = new RepeatRule("Repeat", null, oneCharRule, null, null) as IRule;
            var match = rule.Match(new ExplorerContext("ggggg")).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Rule");
            Assert.AreEqual(5, match.Text.Length, "MatchLength");
            Assert.AreEqual(5, ToList(match.ComputeOutput()).Count(), "Contents");
        }

        [TestMethod]
        public void RepeatCharacterWithCardinality()
        {
            var oneCharRule = new LiteralRule("oneChar", null, "g");
            var testSet = new[]
            {
                Tuple.Create<string, int?, int?, bool>("gg", 2, 2, true),
                Tuple.Create<string, int?, int?, bool>("gg", 1, 2, true),
                Tuple.Create<string, int?, int?, bool>("gg", null, 2, true),
                Tuple.Create<string, int?, int?, bool>("gg", 2, null, true),
                Tuple.Create<string, int?, int?, bool>("ggg", 2, 2, false),
                Tuple.Create<string, int?, int?, bool>("ggg", 2, 3, true),
                Tuple.Create<string, int?, int?, bool>("g", 2, 2, false),
                Tuple.Create<string, int?, int?, bool>("g", 2, 3, false)
            };

            for (int i = 0; i != testSet.Length; ++i)
            {
                var text = testSet[i].Item1;
                var min = testSet[i].Item2;
                var max = testSet[i].Item3;
                var isSuccess = testSet[i].Item4;
                var rule = new RepeatRule("Repeat", null, oneCharRule, min, max) as IRule;
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
                    Assert.AreEqual(text.Length, ToList(match.ComputeOutput()).Count(), $"Contents - {i}");
                }
            }
        }

        [TestMethod]
        public void RepeatWithSequenceAndInterleave()
        {
            var interleave = new RepeatRule("interleave", null, new LiteralRule(null, null, " "), 0, null);
            var seq = new SequenceRule("seq", null, new[]
            {
                new TaggedRule(new LiteralRule(null, null, "|")),
                new TaggedRule("t", new LiteralRule(null, null, "a"))
            });
            var rule = new RepeatRule("rep", null, seq, 1, null);
            var text = "|a  |  a   | a";
            var match = rule.Match(new ExplorerContext(text, interleave)).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Rule");
            Assert.AreEqual(text.Length, match.Text.Length, "MatchLength");
            Assert.AreEqual(3, ToList(match.ComputeOutput()).Count(), "Contents");
        }
   }
}