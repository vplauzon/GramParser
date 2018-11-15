using System;

namespace PasApiClient
{
    /// <summary>Client component for accessing Parser as a Service.</summary>
    public class ParserClient
    {
        private static readonly Uri DEFAULT_BASE_URI = new Uri("http://pas-api.vplauzon.com");
        private readonly Uri _baseUri;

        #region Constructors
        public static ParserClient Create()
        {
            return CreateFromBaseUrl(DEFAULT_BASE_URI);
        }

        public static ParserClient CreateFromBaseUrl(Uri baseUri)
        {
            return new ParserClient(baseUri);
        }

        private ParserClient(Uri baseUri)
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }
        #endregion
    }
}