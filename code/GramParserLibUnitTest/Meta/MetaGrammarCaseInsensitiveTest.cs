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
    public class MetaGrammarCaseInsensitiveTest : MetaGrammarBaseTest
    {
        [TestMethod]
        public void Letter()
        {
            Test(
                (true, "letter", "a"),
                (true, "letter", "A"),
                (false, "letter", "b"));
        }

        [TestMethod]
        public void Range()
        {
            Test(
                (true, "range", "a"),
                (true, "range", "C"),
                (true, "range", "d"),
                (true, "range", "F"),
                (false, "range", "h"),
                (false, "range", "G"));
        }

        private void Test(params (bool isSuccess, string ruleName, string text)[] samples)
        {
            Test("CaseInsensitive.CaseInsensitive.txt", samples);
        }
    }
}