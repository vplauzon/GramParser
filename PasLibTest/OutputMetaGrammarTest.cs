using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasLib;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.Json;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace PasLibTest
{
    [TestClass]
    public class OutputMetaGrammarTest
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

            Test("Identifier.This.txt", samples);
        }

        [TestMethod]
        public void Children()
        {
            var samples = new[]
            {
                (true, "justA", "aaabbbb", (object)"aaa"),
                (true, "justA", "aaa", "aaa"),
                (true, "justA", "abbbb", "a"),
                (true, "justA", "bbbb", ""),
                (true, "justB", "aaabb", "bb")
            };

            Test("Identifier.Children.txt", samples);
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
                (true, "this", "Hello", (object)new[]{ "Hello", "Hello", "Hello"})
            };

            Test("Array.ThisArrays.txt", samples);
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
                (true, "seq", "aaabb", (object)new{ a="aaa", b="bb"}),
                (true, "dij", "aaa", (object)new{ a="aaa"}),
                (true, "dij", "bbbb", (object)new{ b="bbbb"})
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
        #endregion

        private string GetResource(string resourceName)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var fullResourceName = "PasLibTest.Output." + resourceName;

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                return text;
            }
        }

        private void Test(
            string grammarFile,
            (bool isSuccess, string ruleName, string text, object output)[] samples)
        {
            var grammarText = GetResource(grammarFile);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar couldn't get parsed");
            for (int i = 0; i != samples.Length; ++i)
            {
                (var isSuccess, var ruleName, var text, var output) = samples[i];
                var match = grammar.Match(ruleName, text);

                Assert.AreEqual(isSuccess, match != null, $"Success - {i}");

                if (isSuccess)
                {
                    Assert.AreEqual(ruleName, match.Rule.RuleName, $"Rule Name - {i}");
                    Assert.AreEqual(text, match.Text.ToString(), $"Text - {i}");

                    if (match.Output == null)
                    {
                        Assert.AreEqual(output, match.Output, $"Output Null - {i}");
                    }
                    else
                    {
                        string outputText = StandardizeObject(output);
                        var matchOutputText = StandardizeObject(match.Output);

                        Assert.AreEqual(outputText, matchOutputText, $"Output JSON Compare - {i}");
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