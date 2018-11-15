using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasApiClient;
using System;
using System.Threading.Tasks;

namespace PasApiClientTest
{
    [TestClass]
    public class ParserClientMultipleTest : TestBase
    {
        [TestMethod]
        public async Task Simple()
        {
            var client = CreateClient();
            var grammar = "rule letter = \"a\"..\"z\" | \"A\"..\"Z\";rule main = letter*;";
            var text = "abc";
            var result = await client.MultipleParseAsync(grammar, new[] { text });

            Assert.AreEqual(result.Length, 1, "result.Length");
            Assert.IsTrue(result[0].IsMatch, "IsMatch");
            Assert.AreEqual(text, result[0].RuleMatch.Text, "Text");
            Assert.AreEqual(3, result[0].RuleMatch.Children.Length, "Children");
        }

        //[TestMethod]
        //public async Task NoParsing()
        //{
        //    var client = CreateClient();
        //    var grammar = "rule letter = \"a\"..\"z\" | \"A\"..\"Z\";rule main = letter*;";
        //    var text = "123";
        //    var result = await client.SingleParseAsync(grammar, text);

        //    Assert.IsFalse(result.IsMatch, "IsMatch");
        //}

        //[TestMethod]
        //public async Task FailGrammar()
        //{
        //    try
        //    {
        //        var client = CreateClient();
        //        var grammar = "Hello World";
        //        var text = "123";

        //        await client.SingleParseAsync(grammar, text);
        //    }
        //    catch (ParsingException ex)
        //    {
        //        Assert.AreEqual(400, ex.StatusCode, "StatusCode");
        //    }
        //}
    }
}
