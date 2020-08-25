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
            Assert.IsNotNull(result.RuleMatch, "IsMatch not null");
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
            Assert.IsNotNull(result.RuleMatch, "IsMatch not null");
            Assert.AreEqual(text, result.RuleMatch.Text, "Text");
            Assert.IsNull(result.RuleMatch.Children, "Children");
            Assert.AreEqual(1, result.RuleMatch.NamedChildren.Count, "NamedChildren");
            Assert.AreEqual("l", result.RuleMatch.NamedChildren.Keys.First(), "Name");
        }

        [TestMethod]
        public async Task WithOutput()
        {
            var client = CreateClient();
            var grammar = "rule main = \"a\"..\"c\" => [text];";
            var text = "b";
            var result = await client.SingleParseAsync(grammar, text);

            Assert.IsTrue(result.IsMatch, "IsMatch");
            Assert.IsNull(result.RuleMatch, "IsMatch null");

            var output = result.Output;

            Assert.IsNotNull(output, "Output null");
            Assert.IsInstanceOfType(output, typeof(object[]));
            Assert.AreEqual(1, ((object[])output), "Length");
            Assert.AreEqual("b", ((object[])output)[0], "Value");
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
