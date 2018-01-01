using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasLib;
using System.Linq;
using System.Reflection;
using System.IO;

namespace PasLibTest
{
    [TestClass]
    public class MetaGrammarTest
    {
        #region Just parsing the grammar
        [TestMethod]
        public void ParsingSequence()
        {
            var ruleSet = MetaGrammar.ParseGrammar("rule seq = \"a\" \"b\";");

            Assert.IsNotNull(ruleSet);
        }
        #endregion

        #region Simple Tests with grammar files
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
        #endregion

        #region Ad hoc tests with files
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
        #endregion

        #region Children
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
                Assert.IsNull(match.NamedChildren, "NamedChildren - " + rule);
                Assert.IsNotNull(match.Children, "Children - " + rule);
                Assert.AreEqual(expectedChildren, match.Children.Count(), "#Children - " + rule);
            }

            {
                var rule = "without";
                var match = grammar.Match("without", text);

                Assert.IsNotNull(match, "Match - " + rule);
                Assert.IsNull(match.NamedChildren, "NamedChildren - " + rule);
                Assert.IsNotNull(match.Children, "Children - " + rule);
                Assert.AreEqual(0, match.Children.Count(), "#Children - " + rule);
            }
        }
        #endregion

        #region Select Children
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
        #endregion

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

        private void Test(
            string grammarFile,
            (bool isSuccess, string ruleName, string text)[] samples)
        {
            var grammarText = GetResource(grammarFile);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar couldn't get parsed");
            for (int i = 0; i != samples.Length; ++i)
            {
                (var isSuccess, var ruleName, var text) = samples[i];
                var match = grammar.Match(ruleName, text);

                Assert.AreEqual(isSuccess, match != null, $"Success - {i}");

                if (isSuccess)
                {
                    Assert.AreEqual(ruleName, match.Rule.RuleName, $"Rule Name - {i}");
                    Assert.AreEqual(text, match.Text.ToString(), $"Content - {i}");
                }
            }
        }
    }
}