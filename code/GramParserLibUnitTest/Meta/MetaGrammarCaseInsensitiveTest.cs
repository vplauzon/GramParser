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

        [TestMethod]
        public void LetterRepeat()
        {
            Test(
                (true, "letterRepeat", ""),
                (true, "letterRepeat", "f"),
                (true, "letterRepeat", "ff"),
                (true, "letterRepeat", "fff"),
                (true, "letterRepeat", "fFf"),
                (true, "letterRepeat", "F"),
                (true, "letterRepeat", "FF"));
        }

        [TestMethod]
        public void RangeRepeat()
        {
            Test(
                (true, "rangeRepeat", ""),
                (true, "rangeRepeat", "d"),
                (true, "rangeRepeat", "f"),
                (true, "rangeRepeat", "g"),
                (false, "rangeRepeat", "h"),
                (true, "rangeRepeat", "G"),
                (true, "rangeRepeat", "D"),
                (true, "rangeRepeat", "E"),
                (true, "rangeRepeat", "dEfGGfe"));
        }

        [TestMethod]
        public void LetterSequence()
        {
            Test(
                (true, "letterSequence", "op"),
                (true, "letterSequence", "oP"),
                (true, "letterSequence", "Op"),
                (true, "letterSequence", "OP"));
        }

        private void Test(params (bool isSuccess, string ruleName, string text)[] samples)
        {
            Test("CaseInsensitive.CaseInsensitive.txt", samples);
        }
    }
}