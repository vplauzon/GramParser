using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using GramParserLib.Rule;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Rule
{
    [TestClass]
    public class RecursionRuleTest : BaseTest
    {
        [TestMethod]
        public void PotentialInfiniteRecurse()
        {
            //  Equivalent to:
            //  rule A = "a".."z";
            //  rule B = C "," C;
            //  rule C = A | B;
            //  Try to match "a" with C
            var ruleA = new RangeRule("A", null, 'a', 'z');
            var proxyC = new RuleProxy();
            var ruleB = new SequenceRule("B", null, new[]{
                new TaggedRule(proxyC),
                new TaggedRule(new LiteralRule(null, null, ",")),
                new TaggedRule(proxyC)
            });
            var ruleC = new DisjunctionRule("C", null, new[] {
                new TaggedRule(ruleA),
                new TaggedRule(ruleB)
            });

            proxyC.ReferencedRule = ruleC;

            var match = ruleC.Match(new ExplorerContext("a")).FirstOrDefault();

            Assert.IsNotNull(match, "Should be a success");
        }
   }
}