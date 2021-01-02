using GramParserLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            var output = TestTypedOutput<FunctionInvocation>("FunctionInvocation.txt", "f()");

            Assert.AreEqual("f", output.FunctionName, "Function name");
            Assert.AreEqual(0, output.Parameters.Count, "# parameters");
        }

        [TestMethod]
        public void OneParameter()
        {
            var output = TestTypedOutput<FunctionInvocation>(
                "FunctionInvocation.txt",
                "g(x)");

            Assert.AreEqual("g", output.FunctionName, "Function name");
            Assert.AreEqual(1, output.Parameters.Count, "# parameters");
            Assert.AreEqual("x", output.Parameters.First(), "1st parameter");
        }

        [TestMethod]
        public void TwoParameters()
        {
            var output = TestTypedOutput<FunctionInvocation>(
                "FunctionInvocation.txt",
                "h(x,y)");

            Assert.AreEqual("h", output.FunctionName, "Function name");
            Assert.AreEqual(2, output.Parameters.Count, "# parameters");
            Assert.AreEqual("x", output.Parameters[0], "1st parameter");
            Assert.AreEqual("y", output.Parameters[1], "2nd parameter");
        }

        [TestMethod]
        public void ThreeParameters()
        {
            var output = TestTypedOutput<FunctionInvocation>(
                "FunctionInvocation.txt",
                "i(x,y,z)");

            Assert.AreEqual("i", output.FunctionName, "Function name");
            Assert.AreEqual(3, output.Parameters.Count, "# parameters");
            Assert.AreEqual("x", output.Parameters[0], "1st parameter");
            Assert.AreEqual("y", output.Parameters[1], "2nd parameter");
            Assert.AreEqual("z", output.Parameters[2], "3rd parameter");
        }
    }
}