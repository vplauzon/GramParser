using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasLib;
using System.Linq;
using System.Reflection;
using System.IO;

namespace PasLibTest
{
    //  Let's ignore those until replatform
    [Ignore]
    [TestClass]
    public class MetaGrammarTest
    {
        #region Meta Grammar sub rules
        [TestMethod]
        public void TwoDots()
        {
            var dots = new RepeatRule(
                null, null, new LiteralRule(null, null, "."), 2, 2, false, false);
            var result = dots.Match("..", TracePolicy.NoTrace);

            Assert.IsTrue(result.IsSuccess, "Success");
            Assert.AreEqual("..", result.Match.Content, "Content");
        }
        #endregion

        [TestMethod]
        public void Literal()
        {
            var samples = new[]
            {
                Tuple.Create(true, "a", "a"),
                Tuple.Create(false, "a", "b"),
                Tuple.Create(true, "c", "c"),
                Tuple.Create(false, "c", "d"),
                Tuple.Create(true, "F", "F"),
                Tuple.Create(false, "F", "f"),
                Tuple.Create(true, "zero", "0"),
                Tuple.Create(false, "zero", "1")
            };

            SimpleTest("LiteralGrammar.txt", samples);
        }

        [TestMethod]
        public void Any()
        {
            var samples = new[]
            {
                Tuple.Create(true, "main", "a"),
                Tuple.Create(true, "main", "B"),
                Tuple.Create(true, "main", "z"),
                Tuple.Create(true, "main", "!"),
                Tuple.Create(true, "main", ";"),
                Tuple.Create(true, "main", " "),
                Tuple.Create(false, "main", "ab"),
                Tuple.Create(false, "main", "")
            };

            SimpleTest("AnyGrammar.txt", samples);
        }

        [TestMethod]
        public void Range()
        {
            var samples = new[]
            {
                Tuple.Create(true, "ac", "a"),
                Tuple.Create(true, "ac", "b"),
                Tuple.Create(true, "ac", "c"),
                Tuple.Create(false, "ac", "d"),
                Tuple.Create(false, "ac", "A"),
                Tuple.Create(true, "DG", "D"),
                Tuple.Create(true, "DG", "F"),
                Tuple.Create(true, "DG", "G"),
                Tuple.Create(false, "DG", "d"),
                Tuple.Create(false, "DG", "C"),
                Tuple.Create(true, "num", "0"),
                Tuple.Create(true, "num", "2"),
                Tuple.Create(true, "num", "6"),
                Tuple.Create(true, "num", "9"),
                Tuple.Create(false, "num", "a"),
                Tuple.Create(false, "num", "Z"),
                Tuple.Create(false, "num", "!")
            };

            SimpleTest("RangeGrammar.txt", samples);
        }

        [TestMethod]
        public void Repeat()
        {
            var samples = new[]
            {
                Tuple.Create(true, "a", ""),
                Tuple.Create(true, "a", "a"),
                Tuple.Create(true, "a", "aa"),
                Tuple.Create(true, "a", "aaa"),
                Tuple.Create(true, "a", "aaaaaaaaaaaaaaaaa"),
                Tuple.Create(false, "a", "b"),
                Tuple.Create(true, "ab", ""),
                Tuple.Create(true, "ab", "ab"),
                Tuple.Create(true, "ab", "abab"),
                Tuple.Create(true, "ab", "ababababababab"),
                Tuple.Create(false, "ab", "abba"),
                Tuple.Create(true, "abc", ""),
                Tuple.Create(true, "abc", "abc"),
                Tuple.Create(true, "abc", "abcabc"),
                Tuple.Create(true, "abc", "abcabcabcabcabcabcabcabcabcabcabcabcabcabc"),
                Tuple.Create(false, "abc", "abccba"),
                Tuple.Create(true, "optional", ""),
                Tuple.Create(true, "optional", "Hi"),
                Tuple.Create(false, "optional", "HiHi"),
                Tuple.Create(false, "atLeast", ""),
                Tuple.Create(true, "atLeast", "ab"),
                Tuple.Create(true, "atLeast", "abab"),
                Tuple.Create(true, "atLeast", "abababababab"),
                Tuple.Create(true, "exact3", "abcabcabc"),
                Tuple.Create(false, "exact3", "abcabc"),
                Tuple.Create(false, "exact3", "abcabcabcabc"),
                Tuple.Create(false, "exact3", ""),
                Tuple.Create(true, "between34", "defdefdef"),
                Tuple.Create(true, "between34", "defdefdefdef"),
                Tuple.Create(false, "between34", "defdefdefdefdef"),
                Tuple.Create(false, "between34", "defdef"),
                Tuple.Create(false, "between34", "")
            };

            SimpleTest("RepeatGrammar.txt", samples);
        }

        [TestMethod]
        public void Disjunction()
        {
            var samples = new[]
            {
                Tuple.Create(true, "ab", "a"),
                Tuple.Create(true, "ab", "b"),
                Tuple.Create(false, "ab", "c"),
                Tuple.Create(true, "abc", "a"),
                Tuple.Create(true, "abc", "b"),
                Tuple.Create(true, "abc", "c"),
                Tuple.Create(false, "abc", "d"),
                Tuple.Create(true, "abcd", "a"),
                Tuple.Create(true, "abcd", "b"),
                Tuple.Create(true, "abcd", "c"),
                Tuple.Create(true, "abcd", "d"),
                Tuple.Create(false, "abcd", "e"),
                Tuple.Create(true, "hiBob", "Hi"),
                Tuple.Create(true, "hiBob", "Bob"),
                Tuple.Create(false, "hiBob", "How")
            };

            SimpleTest("DisjunctionGrammar.txt", samples);
        }

        [TestMethod]
        public void Sequence()
        {
            var samples = new[]
            {
                Tuple.Create(true, "ab", "ab"),
                Tuple.Create(false, "ab", "a"),
                Tuple.Create(true, "abc", "abc"),
                Tuple.Create(false, "abc", "a"),
                Tuple.Create(true, "abcd", "abcd"),
                Tuple.Create(false, "abcd", "a"),
                Tuple.Create(true, "hiBob", "HiBob"),
                Tuple.Create(false, "hiBob", "Hi")
            };

            SimpleTest("SequenceGrammar.txt", samples);
        }

        [TestMethod]
        public void Substract()
        {
            var samples = new[]
            {
                Tuple.Create(true, "a", "a"),
                Tuple.Create(false, "a", "aa"),
                Tuple.Create(true, "a", "aaa"),
                Tuple.Create(true, "hiBang", "Hi!"),
                Tuple.Create(false, "hiBang", "HiBob!"),
            };

            SimpleTest("SubstractGrammar.txt", samples);
        }

        private void SimpleTest(string grammarFile, Tuple<bool, string, string>[] samples)
        {
            var grammar = GetResource(grammarFile);

            for (int i = 0; i != samples.Length; ++i)
            {
                var isSuccess = samples[i].Item1;
                var ruleName = samples[i].Item2;
                var text = samples[i].Item3;
                var ruleSet = MetaGrammar.ParseGrammar(grammar, TracePolicy.NoTrace);
                var result = ruleSet.Match(ruleName, text, TracePolicy.NoTrace);

                Assert.AreEqual(isSuccess, result.IsSuccess, $"Success - {i}");

                if (isSuccess)
                {
                    Assert.AreEqual(ruleName, result.Match.RuleName, $"Rule Name - {i}");
                    Assert.AreEqual(text, result.Match.Content.ToString(), $"Content - {i}");
                }
            }
        }

        private string GetResource(string resourceName)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var fullResourceName = "PasLibTest.Meta." + resourceName;

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                return text;
            }
        }
    }
}