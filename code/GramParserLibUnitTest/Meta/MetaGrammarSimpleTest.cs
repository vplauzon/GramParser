using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Meta
{
    [TestClass]
    public class MetaGrammarSimpleTest : MetaGrammarBaseTest
    {
        [TestMethod]
        public void OneLetterOneRule()
        {
            var samples = new[]
            {
                (false, "main", "a"),
                (false, "main", "V"),
                (false, "main", "vvv"),
                (true, "main", "v"),
                (false, "main", "z"),
            };

            Test("Simple.OneLetterOneRuleGrammar.txt", samples);
        }

        [TestMethod]
        public void Literal()
        {
            var samples = new[]
            {
                (true, "a", "a"),
                (false, "a", "b"),
                (true, "c", "c"),
                (false, "c", "d"),
                (true, "F", "F"),
                (false, "F", "f"),
                (true, "zero", "0"),
                (false, "zero", "1")
            };

            Test("Simple.LiteralGrammar.txt", samples);
        }

        [TestMethod]
        public void EscapeLiteral()
        {
            var samples = new[]
            {
                (true, "ab", "a\nb"),
                (true, "cd", "c\rd"),
                (true, "tab", "e\tf"),
                (true, "slash", "g\\h"),
                (true, "hexa", "i\x51j"),
                (true, "range", "A"),
                (true, "range", "F"),
                (true, "range", "L"),
                (false, "range", "O"),
                (false, "range", "a")
            };

            Test("Simple.EscapeLiteralGrammar.txt", samples);
        }

        [TestMethod]
        public void Any()
        {
            var samples = new[]
            {
                (true, "main", "a"),
                (true, "main", "B"),
                (true, "main", "z"),
                (true, "main", "!"),
                (true, "main", ";"),
                (true, "main", " "),
                (false, "main", "ab"),
                (false, "main", "")
            };

            Test("Simple.AnyGrammar.txt", samples);
        }

        [TestMethod]
        public void Range()
        {
            var samples = new[]
            {
                (true, "ac", "a"),
                (true, "ac", "b"),
                (true, "ac", "c"),
                (false, "ac", "d"),
                (false, "ac", "A"),
                (true, "DG", "D"),
                (true, "DG", "F"),
                (true, "DG", "G"),
                (false, "DG", "d"),
                (false, "DG", "C"),
                (true, "num", "0"),
                (true, "num", "2"),
                (true, "num", "6"),
                (true, "num", "9"),
                (false, "num", "a"),
                (false, "num", "Z"),
                (false, "num", "!")
            };

            Test("Simple.RangeGrammar.txt", samples);
        }

        [TestMethod]
        public void Repeat()
        {
            var samples = new[]
            {
                (true, "a", ""),
                (true, "a", "a"),
                (true, "a", "aa"),
                (true, "a", "aaa"),
                (true, "a", "aaaaaaaaaaaaaaaaa"),
                (false, "a", "b"),
                (true, "ab", ""),
                (true, "ab", "ab"),
                (true, "ab", "abab"),
                (true, "ab", "ababababababab"),
                (false, "ab", "abba"),
                (true, "abc", ""),
                (true, "abc", "abc"),
                (true, "abc", "abcabc"),
                (true, "abc", "abcabcabcabcabcabcabcabcabcabcabcabcabcabc"),
                (false, "abc", "abccba"),
                (true, "optional", ""),
                (true, "optional", "Hi"),
                (false, "optional", "HiHi"),
                (false, "atLeast", ""),
                (true, "atLeast", "ab"),
                (true, "atLeast", "abab"),
                (true, "atLeast", "abababababab"),
                (true, "exact3", "abcabcabc"),
                (false, "exact3", "abcabc"),
                (false, "exact3", "abcabcabcabc"),
                (false, "exact3", ""),
                (true, "between34", "defdefdef"),
                (true, "between34", "defdefdefdef"),
                (false, "between34", "defdefdefdefdef"),
                (false, "between34", "defdef"),
                (false, "between34", ""),
                (false, "min", "gh"),
                (true, "min", "ghgh"),
                (true, "min", "ghghghghghghghgh"),
                (true, "max", ""),
                (true, "max", "ij"),
                (true, "max", "ijijij"),
                (false, "max", "ijijijij"),
                (false, "minmax", "kl"),
                (true, "minmax", "klkl"),
                (true, "minmax", "klklklkl"),
                (false, "minmax", "klklklklkl")
            };

            Test("Simple.RepeatGrammar.txt", samples);
        }

        [TestMethod]
        public void Disjunction3Terms()
        {
            var samples = new[]
            {
                (true, "myrule", "a"),
                (true, "myrule", "b"),
                (true, "myrule", "c"),
                (false, "myrule", "d"),
                (false, "myrule", "aa"),
                (false, "myrule", "B")
            };

            Test("Simple.Disjunction3TermsGrammar.txt", samples);
        }

        [TestMethod]
        public void Disjunction()
        {
            var samples = new[]
            {
                (true, "ab", "a"),
                (true, "ab", "b"),
                (false, "ab", "c"),
                (true, "abc", "a"),
                (true, "abc", "b"),
                (true, "abc", "c"),
                (false, "abc", "d"),
                (true, "abcd", "a"),
                (true, "abcd", "b"),
                (true, "abcd", "c"),
                (true, "abcd", "d"),
                (false, "abcd", "e"),
                (true, "hiBob", "Hi"),
                (true, "hiBob", "Bob"),
                (false, "hiBob", "How")
            };

            Test("Simple.DisjunctionGrammar.txt", samples);
        }

        [TestMethod]
        public void Sequence()
        {
            var samples = new[]
            {
                (true, "ab", "ab"),
                (false, "ab", "a"),
                (true, "abc", "abc"),
                (false, "abc", "a"),
                (true, "abcd", "abcd"),
                (false, "abcd", "a"),
                (true, "hiBob", "HiBob"),
                (false, "hiBob", "Hi")
            };

            Test("Simple.SequenceGrammar.txt", samples);
        }

        [TestMethod]
        public void Substract()
        {
            var samples = new[]
            {
                (true, "a", "a"),
                (false, "a", "aa"),
                (true, "a", "aaa"),
                (true, "hiBang", "Hi"),
                (true, "hiBang", "!"),
                (false, "hiBang", "Bob"),
                (false, "hiBang", "Hi!"),
                (false, "hiBang", "HiBob!")
            };

            Test("Simple.SubstractGrammar.txt", samples);
        }
   }
}