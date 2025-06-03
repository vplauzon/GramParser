using GramParserLib;
using GramParserLib.Rule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GramParserLibUnitTest.Output
{
    [TestClass]
    public class OutputReferenceTest : OutputBaseTest
    {
        [TestMethod]
        public void ReferenceOutputAlteration()
        {
            var grammarText = @"
rule number = (""0""..""9"");
rule integer = ""-""? number+ => integer(text);
rule main = integer => { ""integer"" : defaultOutput };
";
            var grammar = MetaGrammar.ParseGrammar(grammarText);
            var match = grammar!.Match(null, "32");

            Assert.IsNotNull(match);

            var output = match.ComputeOutput();
            var standardizedOutput = StandardizeObject(output);
            var standardizedExpected = StandardizeObject(new { integer = 32 });

            Assert.AreEqual(standardizedOutput, standardizedExpected);
        }
    }
}