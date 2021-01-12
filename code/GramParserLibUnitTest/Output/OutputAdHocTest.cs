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
    public class OutputAdHocTest : OutputBaseTest
    {
        [TestMethod]
        public void ConstantArrays()
        {
            var samples = new[]
            {
                (true, "main", "f(a,b)", (object?)new {functionName="f", parameters=new[]{ "a", "b" } }),
                (true, "main", "f(a)", (object?)new {functionName="f", parameters=new[]{ "a"} }),
                (true, "main", "f", (object?)new {functionName="f", parameters=new object [0] })
            };

            Test("AdHoc.FlattenOutput.txt", samples);
        }
    }
}