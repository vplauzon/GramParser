using GramParserLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GramParserLibUnitTest.TypedOutput
{
    public class TypedOutputBaseTest : BaseTest
    {
        protected T TestTypedOutput<T>(string resourceName, string text)
        {
            var grammarText = GetResource(resourceName);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar couldn't get parsed");

            var match = grammar!.Match(null, text);

            Assert.IsNotNull(match, "No match");

            var output = match!.ComputeTypedOutput<T>();

            return output;
        }
    }
}