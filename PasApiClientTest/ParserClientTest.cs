using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasApiClient;
using System;
using System.Threading.Tasks;

namespace PasApiClientTest
{
    [TestClass]
    public class ParserClientTest
    {
        private const string BASE_URI_KEY = "baseUri";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task SimpleAsync()
        {
            var client = CreateClient();
            var grammar = "rule letter = \"a\"..\"z\" | \"A\"..\"Z\";rule main = letter*;";
            var text = "abc";
            var result = await client.SingleParseAsync(grammar, text);

            Assert.IsTrue(result.IsMatch, "IsMatch");
            Assert.AreEqual(text, result.RuleMatch.Text, "Text");
            Assert.AreEqual(3, result.RuleMatch.Children.Length, "Children");
        }

        private ParserClient CreateClient()
        {
            if (TestContext.Properties.ContainsKey(BASE_URI_KEY))
            {
                var url = TestContext.Properties["baseUri"] as string;
                var baseUri = new Uri(url);

                return ParserClient.CreateFromBaseUri(baseUri);
            }
            else
            {
                return ParserClient.Create();
            }
        }
    }
}
