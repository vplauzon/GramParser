using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Collections.Generic;

namespace GramParserLibUnitTest.Meta
{
    [TestClass]
    public class MetaGrammarSelectChildrenTest : MetaGrammarBaseTest
    {
        [TestMethod]
        public void SelectChildrenDisjunction()
        {
            var samples = new[]
            {
                ("noTag", "aaa", (int?)3, (Dictionary<string, int>?)null),
                ("noTag", "bb", 2, null),
                ("noTag", "", 0, null),

                ("withTag", "aaaaa", null, new Dictionary<string, int> { { "a", 5 } }),
                ("withTag", "b", null, new Dictionary<string, int> { { "b", 1 } }),
                ("withTag", "", null, new Dictionary<string, int> { { "a", 0 } })
            };

            TestSelectChildren("SelectChildren.Disjunction.txt", samples);
        }

        [TestMethod]
        public void SelectChildrenSequence()
        {
            var samples = new[]
            {
                ("both", "aaabb", (int?)null, new Dictionary<string, int> { { "a", 3 }, { "b", 2 } }),
                ("onlyA", "aaabb", null,new Dictionary<string, int> { { "a", 3 } }),
                ("onlyB", "aaabb", null, new Dictionary<string, int> { { "b", 2 } }),
                ("none", "aaabb", 2, null)
            };

            TestSelectChildren("SelectChildren.Sequence.txt", samples);
        }

        private void TestSelectChildren(
            string resourceName,
            (string ruleName, string text, int? childrenCount, Dictionary<string, int>? childrenCountMap)[] samples)
        {
            var grammarText = GetResource(resourceName);
            var grammar = MetaGrammar.ParseGrammar(grammarText);

            Assert.IsNotNull(grammar, "Grammar");

            foreach (var sample in samples)
            {
                var match = grammar!.Match(sample.ruleName, sample.text);
                var sampleName = $"{sample.ruleName}/{sample.text}";

                Assert.IsNotNull(match, $"Match - {sampleName}");

                var output = match!.ComputeOutput();

                if (sample.childrenCountMap == null)
                {
                    var list = ToList(output);

                    Assert.AreEqual(sample.childrenCount, list.Count, $"Children count - {sampleName}");
                }
                else
                {
                    var map = ToMap(output);

                    Assert.IsTrue(
                        new HashSet<string>(map.Keys).SetEquals(new HashSet<string>(sample.childrenCountMap.Keys)),
                        $"Key - {sampleName}");

                    foreach (var key in map.Keys)
                    {
                        var list = ToList(map[key]);

                        Assert.AreEqual(
                            sample.childrenCountMap[key],
                            list.Count,
                            $"Children count -  {sampleName}");
                    }
                }
            }
        }
    }
}