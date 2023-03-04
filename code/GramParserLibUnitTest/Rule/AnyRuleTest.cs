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
    public class AnyRuleTest: BaseTest
    {
        [TestMethod]
        public void Any()
        {
            var rule = new MatchAnyCharacterRule("Any", null) as IRule;
            var samples = new[]
            {
                "g",
                "K",
                "@",
                "*",
                "/"
            };

            for (int i = 0; i != samples.Length; ++i)
            {
                var match =
                    rule.Match(new ExplorerContext(samples[i])).FirstOrDefault();

                Assert.IsNotNull(match, $"Success - {i}");
                Assert.AreEqual(rule.RuleName, match!.Rule.RuleName, $"Rule - {i}");
                Assert.AreEqual(samples[i].Length, match.Text.Length, $"MatchLength - {i}");
                Assert.AreEqual(1, match.Text.Length, $"Content - {i}");
            }
        }
   }
}