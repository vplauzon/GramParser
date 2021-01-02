using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using GramParserLib.Rule;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Rule
{
    [TestClass]
    public class SequenceRuleTest : BaseTest
    {
        [TestMethod]
        public void SequenceWithoutTags()
        {
            var rule = new SequenceRule("Seq", null, new[]
            {
                new TaggedRule(new LiteralRule(null, null, "Hi")),
                new TaggedRule(new LiteralRule(null, null, "Bob")),
                new TaggedRule(new LiteralRule(null, null, "!"))
            });
            var text = "HiBob!";
            var match = rule.Match(new ExplorerContext(text)).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Seq");
            Assert.AreEqual(text.Length, match.Text.Length, "MatchLength");
            Assert.AreEqual(text.ToString().TrimStart(), match.Text.ToString(), "Content");
            Assert.AreEqual(3, ToList(match.ComputeOutput()).Count, "Fragments");
        }

        [TestMethod]
        public void SequenceWithTags()
        {
            var rule = new SequenceRule("Seq", null, new[]
            {
                new TaggedRule("h", new LiteralRule(null, null, "Hi")),
                new TaggedRule("b", new LiteralRule(null, null, "Bob")),
                new TaggedRule(new LiteralRule(null, null, "!"))
            });
            var text = "HiBob!";
            var match = rule.Match(new ExplorerContext(text)).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Seq");
            Assert.AreEqual(text.Length, match.Text.Length, "MatchLength");

            var output = ToMap(match.ComputeOutput());

            Assert.AreEqual(2, output.Count(), "Fragments");
            Assert.AreEqual("Hi", output["h"].ToString(), "Fragments - Hi");
            Assert.AreEqual("Bob", output["b"].ToString(), "Fragments - Bob");
        }

        [TestMethod]
        public void SequenceWithNoChildrenTags()
        {
            var rule = new SequenceRule("Seq", null, new[]
            {
                new TaggedRule("a", new RepeatRule(null, null, new LiteralRule(null, null, "a"), 1, null)),
                new TaggedRule("b", new RepeatRule(null, null, new LiteralRule(null, null, "b"), 1, null))
            });
            var text = "aaaabb";
            var match = rule.Match(new ExplorerContext(text)).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Seq");
            Assert.AreEqual(text.Length, match.Text.Length, "MatchLength");

            var output = ToMap(match.ComputeOutput());

            Assert.AreEqual(2, output.Count(), "Fragments");
            Assert.AreEqual(
                4, ToList(output["a"]).Count, "Fragments text - a");
            Assert.AreEqual(
                2, ToList(output["b"]).Count(), "Fragments text - b");
        }

        [TestMethod]
        public void SequenceWithInterleave()
        {
            var interleave = new LiteralRule(null, null, " ");
            var rule = new SequenceRule("seq", null, new[]
            {
                new TaggedRule(new LiteralRule(null, null, "|")),
                new TaggedRule("t", new LiteralRule(null, null, "a"))
            });
            var text = "  | a  ";
            var match = rule.Match(new ExplorerContext(text, interleave)).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "seq");
            Assert.AreEqual(1, ToMap(match.ComputeOutput()).Count(), "Contents");
        }
    }
}