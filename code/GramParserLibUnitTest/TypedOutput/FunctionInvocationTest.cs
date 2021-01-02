using GramParserLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GramParserLibUnitTest.TypedOutput
{
    [TestClass]
    public class FunctionInvocationTest : TypedOutputBaseTest
    {
        #region Inner Types
        private class FunctionInvocation
        {
            public string FunctionName { get; set; } = string.Empty;

            public IImmutableList<string> Parameters { get; set; } = ImmutableList<string>.Empty;
        }
        #endregion

        [TestMethod]
        public void NoParameters()
        {
            var output = TestTypedOutput<FunctionInvocation>("FunctionInvocation.txt");

            Assert.AreEqual("f", output.FunctionName, "Function name");
            Assert.AreEqual(0, output.Parameters.Count, "# parameters");
        }
    }
}