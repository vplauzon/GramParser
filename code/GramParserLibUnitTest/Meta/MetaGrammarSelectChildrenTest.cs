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
    public class MetaGrammarSelectChildrenTest : MetaGrammarBaseTest
    {
        [TestMethod]
        public void SelectChildrenDisjunction()
        {
            var samples = new[]
            {
                ("both", "aaa", new []{ 3}),
                ("both", "bbbb", new []{ 4}),
                ("bothATruncated", "aaa", new []{ 0}),
                ("bothBTruncated", "bbbbbbb", new []{ 0}),
                ("a", "aaa", new []{ 3}),
                ("a", "bbbbbbb", new int[]{ }),
                ("aTruncated", "aaaa", new []{ 0}),
                ("aTruncated", "bbbbbbb", new int[]{ }),
                ("b", "aaa", new int[]{ }),
                ("b", "bb", new []{ 2}),
                ("bTruncated", "aaaa", new int[]{ }),
                ("bTruncated", "bbbbbbb", new []{ 0})
            };

            TestSelectChildren("SelectChildren.Disjunction.txt", samples);
        }

        [TestMethod]
        public void SelectChildrenSequence()
        {
            var samples = new[]
            {
                ("both", "aaabb", new []{ 3, 2}),
                ("bothATruncated", "aaabb", new []{ 0, 2}),
                ("bothBTruncated", "aaabb", new []{ 3, 0}),
                ("a", "aaabb", new []{ 3}),
                ("aTruncated", "aaabb", new []{ 0}),
                ("b", "aaabb", new int[]{ 2}),
                ("bTruncated", "aaabb", new []{ 0})
            };

            TestSelectChildren("SelectChildren.Sequence.txt", samples);
        }

        private void TestSelectChildren(
            string resourceName,
            (string ruleName, string text, int[] children)[] samples)
        {
            var grammarText = GetResource(resourceName);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar");
        }
   }
}