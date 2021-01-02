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
    public class OutputArrayTest : OutputBaseTest
    {
        [TestMethod]
        public void ConstantArrays()
        {
            var samples = new[]
            {
                (true, "empty", "Hello", (object?)new object[0]),
                (true, "integers", "Hello", (object)new[]{1,2,3,4,5}),
                (true, "doubles", "Hello", new[]{1.2,2.3,3.4,4.5,5.6}),
                (true, "mixIntegerDoubles", "Hello", new[]{1,2,3.4,4,5.6}),
                (true, "strings", "Hello", new[]{"Hi", "There"}),
                (true, "booleans", "Hello", new[]{true, false}),
                (true, "nulls", "Hello", new object?[]{null, null})
            };

            Test("Array.ConstantArrays.txt", samples);
        }

        [TestMethod]
        public void ThisArrays()
        {
            var samples = new[]
            {
                (true, "text", "Hello", (object?)new[]{ "Hello", "Hello", "Hello"})
            };

            Test("Array.TextArrays.txt", samples);
        }
   }
}