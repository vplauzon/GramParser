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
    public class LiteralRuleTest : BaseTest
    {
        [TestMethod]
        public void Literal()
        {
            var rule = new LiteralRule("Lit", null, "great") as IRule;
            var match = rule.Match(new ExplorerContext("great")).FirstOrDefault();
            var nomatch = rule.Match(new ExplorerContext("h")).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match!.Rule.RuleName, "Rule");
            Assert.AreEqual(5, match.Text.Length, "Content");

            Assert.IsNull(nomatch, "Failure");
        }
   }
}