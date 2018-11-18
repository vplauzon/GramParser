using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasApiClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PasApiClientTest
{
    [TestClass]
    public class ParserClientSingleTest : TestBase
    {
        [TestMethod]
        public async Task Simple()
        {
            var client = CreateClient();
            var grammar = "rule letter = \"a\"..\"z\" | \"A\"..\"Z\";rule main = letter*;";
            var text = "abc";
            var result = await client.SingleParseAsync(grammar, text);

            Assert.IsTrue(result.IsMatch, "IsMatch");
            Assert.AreEqual(text, result.RuleMatch.Text, "Text");
            Assert.AreEqual(3, result.RuleMatch.Children.Length, "Children");
        }

        [TestMethod]
        public async Task NamedChildren()
        {
            var client = CreateClient();
            var grammar = "rule letter = \"a\"..\"z\" | \"A\"..\"Z\";rule number=\"0\"..\"9\";rule main = n:number+ | l:letter*;";
            var text = "abc";
            var result = await client.SingleParseAsync(grammar, text);

            Assert.IsTrue(result.IsMatch, "IsMatch");
            Assert.AreEqual(text, result.RuleMatch.Text, "Text");
            Assert.IsNull(result.RuleMatch.Children, "Children");
            Assert.AreEqual(1, result.RuleMatch.NamedChildren.Count, "NamedChildren");
            Assert.AreEqual("l", result.RuleMatch.NamedChildren.Keys.First(), "Name");
        }

        [TestMethod]
        public async Task NoParsing()
        {
            var client = CreateClient();
            var grammar = "rule letter = \"a\"..\"z\" | \"A\"..\"Z\";rule main = letter*;";
            var text = "123";
            var result = await client.SingleParseAsync(grammar, text);

            Assert.IsFalse(result.IsMatch, "IsMatch");
        }

        [TestMethod]
        public async Task FailGrammar()
        {
            try
            {
                var client = CreateClient();
                var grammar = "Hello World";
                var text = "123";

                await client.SingleParseAsync(grammar, text);
            }
            catch (ParsingException ex)
            {
                Assert.AreEqual(400, ex.StatusCode, "StatusCode");
            }
        }
    }
}
