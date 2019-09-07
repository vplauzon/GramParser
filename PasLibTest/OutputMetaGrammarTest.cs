using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasLib;
using System.Linq;
using System.Reflection;
using System.IO;

namespace PasLibTest
{
    [TestClass]
    public class OutputMetaGrammarTest
    {
        #region Identifiers
        [TestMethod]
        public void This()
        {
            var samples = new[]
            {
                (false, "main", "a"),
                (false, "main", "V"),
                (false, "main", "vvv"),
                (true, "main", "v"),
                (false, "main", "z"),
            };

            Test("Simple.OneLetterOneRuleGrammar.txt", samples);
        }
        #endregion

        private string GetResource(string resourceName)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var fullResourceName = "PasLibTest.Output." + resourceName;

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                return text;
            }
        }

        private void Test(
            string grammarFile,
            (bool isSuccess, string ruleName, string text)[] samples)
        {
            var grammarText = GetResource(grammarFile);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar couldn't get parsed");
            for (int i = 0; i != samples.Length; ++i)
            {
                (var isSuccess, var ruleName, var text) = samples[i];
                var match = grammar.Match(ruleName, text);

                Assert.AreEqual(isSuccess, match != null, $"Success - {i}");

                if (isSuccess)
                {
                    Assert.AreEqual(ruleName, match.Rule.RuleName, $"Rule Name - {i}");
                    Assert.AreEqual(text, match.Text.ToString(), $"Content - {i}");
                }
            }
        }
    }
}