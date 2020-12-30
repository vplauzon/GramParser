using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.Json;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace GramParserLibUnitTest.Output
{
    [TestClass]
    public class OutputMetaGrammarTest : BaseTest
    {
        #region Identifiers
        [TestMethod]
        public void This()
        {
            var samples = new[]
            {
                (false, "literal", "a", (object)null),
                (true, "literal", "ab", "ab"),
                (false, "repeat", "a", null),
                (true, "repeat", "", ""),
                (true, "repeat", "ab", "ab"),
                (true, "repeat", "abab", "abab"),
                (false, "disjunction", "Hi", null),
                (true, "disjunction", "Hello", "Hello"),
                (true, "disjunction", "World", "World"),
                (false, "sequence", "Hi", null),
                (true, "sequence", "Hello World", "Hello World")
            };

            Test("Identifier.Text.txt", samples);
        }

        [TestMethod]
        public void ChildrenIdentifier()
        {
            var samples = new[]
            {
                (true, "justA", "aaabbbb", (object)new[]{"a","a","a" }),
                (true, "justA", "aaa", (object)new[]{"a","a","a" }),
                (true, "justA", "abbbb", (object)new[]{"a" }),
                (true, "justA", "bbbb", new object[0]),
                (true, "justB", "aaabb", (object)new[]{"b","b" }),
                (true, "arrayOf", "a", new object[]{ new[] { "a" }, new object[0]}),
                (true, "range", "b", new object[]{"b"})
            };

            Test("Identifier.Children.txt", samples);
        }

        [TestMethod]
        public void SubItemIdentifier()
        {
            var samples = new[]
            {
                (true, "list", "a", (object)new object[0]),
                (true, "list", "a,b", (object)new []{ "b" }),
                (true, "list", "a,b,c", (object)new []{ "b", "c" })
            };

            Test("Identifier.SubItem.txt", samples);
        }
        #endregion

        #region Constants
        [TestMethod]
        public void Constants()
        {
            var samples = new[]
            {
                (true, "literal", "abab", (object)"constant"),
                (true, "true", "abab", true),
                (true, "false", "abab", false),
                (true, "null", "abab", null),
                (true, "integer", "abab", 1),
                (true, "negativeInteger", "abab", -56),
                (true, "double", "abab", -3.14)
            };

            Test("Constant.Constants.txt", samples);
        }

        [TestMethod]
        public void BracketSubstitution()
        {
            var samples = new[]
            {
                (true, "main", "a", (object)"d"),
                (true, "main", "b", "d")
            };

            Test("Constant.BracketSubstitution.txt", samples);
        }
        #endregion

        #region Arrays
        [TestMethod]
        public void ConstantArrays()
        {
            var samples = new[]
            {
                (true, "empty", "Hello", (object)new object[0]),
                (true, "integers", "Hello", (object)new[]{1,2,3,4,5}),
                (true, "doubles", "Hello", new[]{1.2,2.3,3.4,4.5,5.6}),
                (true, "mixIntegerDoubles", "Hello", new[]{1,2,3.4,4,5.6}),
                (true, "strings", "Hello", new[]{"Hi", "There"}),
                (true, "booleans", "Hello", new[]{true, false}),
                (true, "nulls", "Hello", new object[]{null, null})
            };

            Test("Array.ConstantArrays.txt", samples);
        }

        [TestMethod]
        public void ThisArrays()
        {
            var samples = new[]
            {
                (true, "text", "Hello", (object)new[]{ "Hello", "Hello", "Hello"})
            };

            Test("Array.TextArrays.txt", samples);
        }
        #endregion

        #region Objects
        [TestMethod]
        public void ConstantObjects()
        {
            var samples = new[]
            {
                (true, "empty", "hi", new object()),
                (true, "oneField", "hi", new { text="Hello" }),
                (true, "twoFields", "hi", ImmutableDictionary<string, object>.Empty.Add("text", "Hello").Add("number", 42)),
                (true, "threeFields", "hi", ImmutableDictionary<string, object>.Empty.Add("text", "Hello").Add("number", -42).Add("boolean", true))
            };

            Test("Object.ConstantObjects.txt", samples);
        }

        [TestMethod]
        public void ChildrenObject()
        {
            var samples = new[]
            {
                (true, "seq", "aaabb", (object)new{ a=new[]{"a","a","a" }, b=new[]{"b","b" } }),
                (true, "dij", "aaa", new {a=new[]{"a","a","a" } }),
                (true, "dij", "bbbb", new {b=new[]{"b","b","b", "b" } })
            };

            Test("Object.Children.txt", samples);
        }
        #endregion

        #region Functions
        [TestMethod]
        public void ConstantFunctions()
        {
            var samples = new[]
            {
                (true, "oneParam", "Hello", (object)42 ),
                (true, "manyParams", "Hello", "HiMyNameIsMax")
            };

            Test("Function.ConstantFunctions.txt", samples);
        }

        [TestMethod]
        public void IntegerFunctions()
        {
            var samples = new[]
            {
                (true, "singleDigit", "0", (object)0),
                (true, "singleDigit", "4", 4),
                (true, "singleDigit", "9", 9),
                (true, "manyDigits", "12", 12),
                (true, "manyDigits", "012", 12),
                (true, "manyDigits", "00012", 12),
                (true, "manyDigits", "7897", 7897)
            };

            Test("Function.IntegerFunctions.txt", samples);
        }

        [TestMethod]
        public void BooleanFunctions()
        {
            var samples = new[]
            {
                (true, "bool", "true", (object)true),
                (true, "bool", "false", (object)false)
            };

            Test("Function.BooleanFunctions.txt", samples);
        }

        [TestMethod]
        public void WithTextFunctions()
        {
            var samples = new[]
            {
                (true, "int", "74", (object)74)
            };

            Test("Function.WithTextFunctions.txt", samples);
        }

        [TestMethod]
        public void PrependFunctions()
        {
            var samples = new[]
            {
                (true, "list", "a", (object)new[]{"a"}),
                (true, "list", "a,b", (object)new[]{"a", "b"}),
                (true, "list", "a,b,c", (object)new[]{"a", "b", "c"})
            };

            Test("Function.Prepend.txt", samples);
        }
        #endregion

        private void Test(
            string grammarFile,
            (bool isSuccess, string ruleName, string text, object expectedOutput)[] samples)
        {
            var grammarText = GetResource(grammarFile);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar couldn't get parsed");
            for (int i = 0; i != samples.Length; ++i)
            {
                (var isSuccess, var ruleName, var text, var expectedOutput) = samples[i];
                var match = grammar.Match(ruleName, text);

                Assert.AreEqual(isSuccess, match != null, $"Success - {i}");

                if (isSuccess)
                {
                    Assert.AreEqual(text, match.Text.ToString(), $"Text - {i}");

                    if (match.ComputeOutput() == null)
                    {
                        Assert.AreEqual(expectedOutput, match.ComputeOutput(), $"Output Null - {i}");
                    }
                    else
                    {
                        string expectedOutputText = StandardizeObject(expectedOutput);
                        var computedOutput = match.ComputeOutput();
                        var matchOutputText = StandardizeObject(computedOutput);

                        Assert.AreEqual(
                            expectedOutputText,
                            matchOutputText,
                            $"Output JSON Compare - {i}");
                    }
                }
            }
        }

        private static string StandardizeObject(object obj)
        {
            var serialized = JsonSerializer.Serialize(obj);

            if (serialized.StartsWith("{"))
            {
                var map = JsonSerializer.Deserialize<IImmutableDictionary<string, object>>(
                    serialized);
                var sortedMap = from p in map
                                orderby p.Key
                                select p;
                var text = JsonSerializer.Serialize(sortedMap);

                return text;
            }
            else
            {
                return serialized;
            }
        }
    }
}