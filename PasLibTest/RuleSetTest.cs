using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasLib;
using System.Linq;

namespace PasLibTest
{
    [Ignore]
    [TestClass]
    public class RuleSetTest
    {
        #region Trivial
        [TestMethod]
        public void FailWhenRubish()
        {
            var interleave = new LiteralRule(null, " ");
            var rule = new RepeatRule(
                "all",
                new LiteralRule(null, "ab"),
                null,
                null,
                false,
                false);
            var ruleSet = new RuleSet(interleave, new[] { rule });
            var text = " ababababkebab  ";
            var result = ruleSet.Match("all", text, RuleSet.DEFAULT_MAX_DEPTH);

            Assert.IsTrue(result.IsFailure, "Failure");
            Assert.AreEqual("kebab  ", result.Unmatched.ToString(), "Unmatched");
        }
        #endregion
    }
}