using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;
using System.Text.Json;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace GramParserLibUnitTest.Output
{
    [TestClass]
    public class OutputIdentifierTest : OutputBaseTest
    {
        [TestMethod]
        public void This()
        {
            var samples = new[]
            {
                (false, "literal", "a", (object?)null),
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
                (true, "justA", "aaabbbb", (object?)new[]{"a","a","a" }),
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
                (true, "list", "a", (object?)new object[0]),
                (true, "list", "a,b", (object)new []{ "b" }),
                (true, "list", "a,b,c", (object)new []{ "b", "c" })
            };

            Test("Identifier.SubItem.txt", samples);
        }
    }
}