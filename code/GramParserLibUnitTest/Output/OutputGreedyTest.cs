using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;

namespace GramParserLibUnitTest.Output
{
    [TestClass]
    public class OutputGreedyTest : OutputBaseTest
    {
        [TestMethod]
        public void GreedyQuotes()
        {
            var grammarText = GetResource("Greedy.GreedyQuotes.txt");
            var grammar = MetaGrammar.ParseGrammar(grammarText);
            var match = grammar!.Match(
                "main",
                "a=''hello'';b=''bon'jour'';c=''hi'';");

            Assert.IsNotNull(match);

            var output = (IEnumerable<object>) match.ComputeOutput()!;

            Assert.AreEqual(1, output.Count());
        }

        [TestMethod]
        public void NonGreedyQuotes()
        {
            var grammarText = GetResource("Greedy.NonGreedyQuotes.txt");
            var grammar = MetaGrammar.ParseGrammar(grammarText);
            var match = grammar!.Match(
                "main",
                "a=''hello'';b=''bon'jour'';c=''hi'';");

            Assert.IsNotNull(match);

            var output = (IEnumerable<object>)match.ComputeOutput()!;

            Assert.AreEqual(3, output.Count());
        }
    }
}