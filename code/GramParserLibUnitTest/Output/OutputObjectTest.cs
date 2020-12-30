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
    public class OutputObjectTest : OutputBaseTest
    {
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
   }
}