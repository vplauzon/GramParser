using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Meta
{
    [TestClass]
    public class MetaGrammarChildrenTest : MetaGrammarBaseTest
    {
        [TestMethod]
        public void ChildrenRepeat()
        {
            TestChildren("Children.Repeat.txt", "aaaa", 4);
        }

        [TestMethod]
        public void ChildrenDisjunction()
        {
            TestChildren("Children.Disjunction.txt", "aaaa", 1);
        }

        [TestMethod]
        public void ChildrenSequence()
        {
            TestChildren("Children.Sequence.txt", "aaaa", 2);
        }

        [TestMethod]
        public void ChildrenSubstraction()
        {
            TestChildren("Children.Substraction.txt", "aaaa", 1);
        }

        private void TestChildren(string resourceName, string text, int expectedChildren)
        {
            var grammarText = GetResource(resourceName);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            foreach (var rule in new[] { "with", "unspecified" })
            {
                var match = grammar.Match(rule, text);

                Assert.IsNotNull(match, "Match - " + rule);

                var output = match.ComputeOutput() as IImmutableList<object>;

                Assert.IsNotNull(output, "Output null - " + rule);
                Assert.AreEqual(expectedChildren, output.Count(), "#Children - " + rule);
            }

            {
                var rule = "without";
                var match = grammar.Match("without", text);

                Assert.IsNotNull(match, "Match - " + rule);

                var output = match.ComputeOutput() as IImmutableList<object>;

                Assert.IsNotNull(output, "Output null - " + rule);
                Assert.AreEqual(0, output.Count(), "#Children - " + rule);
            }
        }
   }
}