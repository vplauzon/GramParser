using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;
using System.Text.Json;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace GramParserLibUnitTest.Output
{
    public class OutputBaseTest : BaseTest
    {
        protected void Test(
            string grammarFile,
            (bool isSuccess, string ruleName, string text, object? expectedOutput)[] samples)
        {
            var grammarText = GetResource(grammarFile);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar couldn't get parsed");
            for (int i = 0; i != samples.Length; ++i)
            {
                (var isSuccess, var ruleName, var text, var expectedOutput) = samples[i];
                var match = grammar!.Match(ruleName, text);

                Assert.AreEqual(isSuccess, match != null, $"Success - {i}");

                if (isSuccess)
                {
                    Assert.AreEqual(text, match!.Text.ToString(), $"Text - {i}");

                    if (match.ComputeOutput() == null)
                    {
                        Assert.AreEqual(expectedOutput, match.ComputeOutput(), $"Output Null - {i}");
                    }
                    else
                    {
                        string expectedOutputText = StandardizeObject(expectedOutput);
                        var computedOutput = match.ComputeOutput();
                        var matchOutputText = StandardizeObject(computedOutput);

                        Assert.AreEqual(
                            expectedOutputText,
                            matchOutputText,
                            $"Output JSON Compare - {i}");
                    }
                }
            }
        }

        private static string StandardizeObject(object? obj)
        {
            var options = new JsonSerializerOptions();

            options.Converters.Add(SubString.JsonConverter);

            var serialized = JsonSerializer.Serialize(obj, options);

            if (serialized.StartsWith("{"))
            {
                var map = JsonSerializer.Deserialize<IImmutableDictionary<string, object>>(
                    serialized);
                var sortedMap = from p in map
                                orderby p.Key
                                select p;
                var text = JsonSerializer.Serialize(sortedMap);

                return text;
            }
            else
            {
                return serialized;
            }
        }
    }
}