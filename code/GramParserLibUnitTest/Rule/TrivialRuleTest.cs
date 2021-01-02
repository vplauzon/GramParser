using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using GramParserLib.Rule;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Rule
{
    [TestClass]
    public class TrivialRuleTest : BaseTest
    {
        [TestMethod]
        public void EmptyNone()
        {
            var rule = MatchNoneRule.Instance;
            var match = rule.Match(new ExplorerContext("")).FirstOrDefault();

            Assert.IsNull(match);
        }

        [TestMethod]
        public void EmptyAny()
        {
            var rule = new MatchAnyCharacterRule(null, null) as IRule;
            var match = rule.Match(new ExplorerContext("")).FirstOrDefault();

            Assert.IsNull(match);
        }
    }
}