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
    public class MetaGrammarAdHocTest : MetaGrammarBaseTest
    {
        [TestMethod]
        public void TwoRules()
        {
            Test("AdHoc.TwoRules.txt", new[] { (true, "main", "aaaaaaabbbb") });
        }

        [TestMethod]
        public void Comments()
        {
            Test("AdHoc.Comments.txt", new[] { (true, "main", "aaaaaaa") });
        }

        [TestMethod]
        public void Interleave()
        {
            Test("AdHoc.Interleave.txt", new[] { (true, "main", "aaZZZaazZzzaaa") });
        }

        [TestMethod]
        public void RepeatWithInterleaves()
        {
            var samples = new[]
            {
                (true, "a", "aaaaa"),
                (false, "a", "aaazaa"),
                (false, "a", "aaa aa"),
                (false, "a", "aaa    aa"),
                (true, "b", "bbbbbbb"),
                (true, "b", "bbb bb zbb"),
                (true, "c", "cccccccccc"),
                (true, "c", "cccc  cccccc"),
                (true, "c", "cccc  ccczzzccc")
            };

            Test("AdHoc.RepeatWithInterleaves.txt", samples);
        }

        [TestMethod]
        public void Backet()
        {
            Test(
                "AdHoc.Bracket.txt",
                new[]
                {
                    (true, "main", "a"),
                    (true, "main", "b")
                });
        }

        [TestMethod]
        public void NoBlankLines()
        {
            Test(
                "AdHoc.BlankLines.txt",
                new[]
                {
                    (true, "blackBox", "adf"),
                    (true, "blackBoxesSeparatedWithBlankLine", "adf"),
                    (true, "blackBoxesSeparatedWithBlankLine", "adf  \n  \t\njhlk"),
                    (true, "scopedBlackBoxes", "{}")
                });
        }

        [TestMethod]
        public void Parenthesis()
        {
            Test(
                "AdHoc.Parenthesis.txt",
                new[]
                {
                    (true, "main", "1+2"),
                    (true, "main", "(1+2)+3")
                });
        }
    }
}