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
    public class OutputConstantTest : OutputBaseTest
    {
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
   }
}