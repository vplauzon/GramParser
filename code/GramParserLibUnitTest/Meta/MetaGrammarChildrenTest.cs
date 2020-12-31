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
            TestChildren("Children.Disjunction.txt", "aaaa", 4);
        }

        [TestMethod]
        public void ChildrenSequence()
        {
            TestChildren("Children.Sequence.txt", "aaaa", 2);
        }

        [TestMethod]
        public void ChildrenSubstraction()
        {
            TestChildren("Children.Substraction.txt", "aaaa", 4);
        }

        private void TestChildren(string resourceName, string text, int expectedChildren)
        {
            var grammarText = GetResource(resourceName);
            var grammar = MetaGrammar.ParseGrammar(grammarText);
            var match = grammar.Match("main", text);

            Assert.IsNotNull(match, "Match");

            var output = ToList(match.ComputeOutput());

            Assert.IsNotNull(output, "Output null");
            Assert.AreEqual(expectedChildren, output.Count(), "#Children");
        }
    }
}