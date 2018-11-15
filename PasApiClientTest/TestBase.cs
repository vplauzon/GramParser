using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasApiClient;
using System;

namespace PasApiClientTest
{
    public class TestBase
    {
        private const string BASE_URI_KEY = "baseUri";

        public TestContext TestContext { get; set; }

        protected ParserClient CreateClient()
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