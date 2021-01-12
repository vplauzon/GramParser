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
    public class OutputFunctionTest : OutputBaseTest
    {
        [TestMethod]
        public void ConstantFunction()
        {
            var samples = new[]
            {
                (true, "oneParam", "Hello", (object?)42 ),
                (true, "manyParams", "Hello", "HiMyNameIsMax")
            };

            Test("Function.ConstantFunctions.txt", samples);
        }

        [TestMethod]
        public void IntegerFunction()
        {
            var samples = new[]
            {
                (true, "singleDigit", "0", (object?)0),
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
        public void BooleanFunction()
        {
            var samples = new[]
            {
                (true, "bool", "true", (object?)true),
                (true, "bool", "false", (object)false)
            };

            Test("Function.BooleanFunctions.txt", samples);
        }

        [TestMethod]
        public void WithTextFunction()
        {
            var samples = new[]
            {
                (true, "int", "74", (object?)74)
            };

            Test("Function.WithTextFunctions.txt", samples);
        }

        [TestMethod]
        public void PrependFunction()
        {
            var samples = new[]
            {
                (true, "list", "a", (object?)new[]{"a"}),
                (true, "list", "a,b", (object)new[]{"a", "b"}),
                (true, "list", "a,b,c", (object)new[]{"a", "b", "c"})
            };

            Test("Function.Prepend.txt", samples);
        }

        [TestMethod]
        public void FlattenFunction()
        {
            var samples = new[]
            {
                (true, "flat", "ab", (object?)new[]{"a", "b"}),
                (true, "flat", "aab", (object)new[]{ "a", "a", "b"}),
                (true, "flat", "aabbb", (object)new[]{ "a", "a", "b", "b", "b"})
            };

            Test("Function.Flatten.txt", samples);
        }
    }
}