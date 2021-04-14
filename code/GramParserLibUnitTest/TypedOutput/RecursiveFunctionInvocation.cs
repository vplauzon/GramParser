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
    public class RecursiveFunctionInvocation : TypedOutputBaseTest
    {
        #region Inner Types
        private class Parameter
        {
            public string? Id { get; set; } = null;

            public Invocation? Invocation { get; set; } = null;
        }

        private class Invocation
        {
            public string FunctionName { get; set; } = string.Empty;

            public IImmutableList<Parameter> Parameters { get; set; } = ImmutableList<Parameter>.Empty;
        }
        #endregion

        [TestMethod]
        public void NoParameters()
        {
            var output = TestTypedOutput<Invocation>("RecursiveFunctionInvocation.txt", "f()");

            Assert.IsNotNull(output, "Output");
            Assert.AreEqual("f", output!.FunctionName, "Function name");
            Assert.AreEqual(0, output!.Parameters.Count, "# parameters");
        }

        [TestMethod]
        public void OneSimpleParameter()
        {
            var output = TestTypedOutput<Invocation>(
                "RecursiveFunctionInvocation.txt",
                "g(x)");

            Assert.IsNotNull(output, "Output");
            Assert.AreEqual("g", output!.FunctionName, "Function name");
            Assert.AreEqual(1, output!.Parameters.Count, "# parameters");
            Assert.AreEqual("x", output!.Parameters[0].Id, "1st parameter");
        }

        [TestMethod]
        public void OneSimpleParameterOneComplexOne()
        {
            var output = TestTypedOutput<Invocation>(
                "RecursiveFunctionInvocation.txt",
                "h(x,f(y))");

            Assert.IsNotNull(output, "Output");
            Assert.AreEqual("h", output!.FunctionName, "Function name");
            Assert.AreEqual(2, output!.Parameters.Count, "# parameters");
            Assert.AreEqual("x", output!.Parameters[0].Id, "1st parameter");
            Assert.IsNotNull(output!.Parameters[1].Invocation, "2nd parameter invocation");
            Assert.AreEqual("f", output!.Parameters[1].Invocation!.FunctionName, "2nd parameter function name");
            Assert.AreEqual(1, output!.Parameters[1].Invocation!.Parameters.Count, "# parameters on 2nd parameter");
            Assert.AreEqual("y", output!.Parameters[1].Invocation!.Parameters[0].Id, "1st parameter of 2nd parameter");
        }

        [TestMethod]
        public void OneSimpleParameterOneComplexOneOneSuperComplex()
        {
            var output = TestTypedOutput<Invocation>(
                "RecursiveFunctionInvocation.txt",
                "i(x,f(y),g(f(y)))");

            Assert.IsNotNull(output, "Output");
            Assert.AreEqual("i", output!.FunctionName, "Function name");
            Assert.AreEqual(3, output!.Parameters.Count, "# parameters");
            Assert.AreEqual("x", output!.Parameters[0].Id, "1st parameter");

            Assert.IsNotNull(output!.Parameters[1].Invocation, "2nd parameter invocation");
            Assert.AreEqual("f", output!.Parameters[1].Invocation!.FunctionName, "2nd parameter function name");
            Assert.AreEqual(1, output!.Parameters[1].Invocation!.Parameters.Count, "# parameters on 2nd parameter");
            Assert.AreEqual("y", output!.Parameters[1].Invocation!.Parameters[0].Id, "1st parameter of 2nd parameter");

            Assert.IsNotNull(output!.Parameters[2].Invocation, "3rd parameter invocation");
            Assert.AreEqual("g", output!.Parameters[2].Invocation!.FunctionName, "3rd parameter function name");
            Assert.AreEqual(1, output!.Parameters[2].Invocation!.Parameters.Count, "# parameters on 3rd parameter");
            Assert.IsNotNull(output!.Parameters[2].Invocation!.Parameters[0].Invocation, "1st parameter of 3rd parameter invocation");
            Assert.AreEqual("f", output!.Parameters[2].Invocation!.Parameters[0].Invocation!.FunctionName, "1st parameter of 3rd parameter");
        }
    }
}