using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Meta
{
    public class MetaGrammarBaseTest : BaseTest
    {
        protected void Test(
            string grammarFile,
            params (bool isSuccess, string ruleName, string text)[] samples)
        {
            var grammarText = GetResource(grammarFile);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar couldn't get parsed");
            for (int i = 0; i != samples.Length; ++i)
            {
                (var isSuccess, var ruleName, var text) = samples[i];
                var match = grammar!.Match(ruleName, text);

                Assert.AreEqual(isSuccess, match != null, $"Success - {i}");

                if (isSuccess)
                {
                    Assert.AreEqual(text, match!.Text.ToString(), $"Content - {i}");
                }
            }
        }
    }
}