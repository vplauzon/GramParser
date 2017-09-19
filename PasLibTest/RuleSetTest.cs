using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasLib;
using System.Linq;

namespace PasLibTest
{
    [TestClass]
    public class RuleSetTest
    {
        #region Trivial
        [TestMethod]
        public void CompleteInterleaves()
        {
            var interleave = new LiteralRule(null, null, " ");
            var rule = new RepeatRule(
                "all",
                interleave,
                new LiteralRule(null, null, "ab"),
                null,
                null,
                false,
                false);
            var ruleSet = new RuleSet(interleave, new[] { rule });
            var text = " abababab  ";
            var result = ruleSet.Match("all", text);

            Assert.IsTrue(result.IsSuccess, "Success");
            Assert.AreEqual(text.Length, result.Match.MatchLength, "MatchLength");
            Assert.AreEqual(text.Trim(), result.Match.Content.ToString(), "Content");
        }

        [TestMethod]
        public void FailWhenRubish()
        {
            var interleave = new LiteralRule(null, null, " ");
            var rule = new RepeatRule(
                "all",
                interleave,
                new LiteralRule(null, null, "ab"),
                null,
                null,
                false,
                false);
            var ruleSet = new RuleSet(interleave, new[] { rule });
            var text = " ababababkebab  ";
            var result = ruleSet.Match("all", text);

            Assert.IsTrue(result.IsFailure, "Failure");
            Assert.AreEqual("kebab  ", result.Unmatched.ToString(), "Unmatched");
        }
        #endregion
    }
}