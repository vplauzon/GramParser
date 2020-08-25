using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PasApiClient
{
    /// <summary>Client component for accessing Parser as a Service.</summary>
    public class ParserClient
    {
        #region Inner Types
        private class SingleParsingInput
        {
            public string? Grammar { get; set; }

            public string? Rule { get; set; }

            public string? Text { get; set; }
        }

        private class MultipleParsingInput
        {
            public string? Grammar { get; set; }

            public string? Rule { get; set; }

            public string[]? Texts { get; set; }
        }
        #endregion

        private static readonly Uri DEFAULT_BASE_URI = new Uri("http://pas-api.vincentlauzon.com");
        private readonly Uri _baseUri;

        #region Constructors
        /// <summary>Creates a client targetting default API URL.</summary>
        /// <returns>Instance of a client.</returns>
        public static ParserClient Create()
        {
            return CreateFromBaseUri(DEFAULT_BASE_URI);
        }

        /// <summary>Creates a client targetting a given API URL.</summary>
        /// <remarks>
        /// This method is useful to target different version of the API.
        /// It is mostly used for internal development of this NUGET package.
        /// </remarks>
        /// <param name="baseUri">Location of the service.</param>
        /// <returns>Instance of a client.</returns>
        public static ParserClient CreateFromBaseUri(Uri baseUri)
        {
            return new ParserClient(baseUri);
        }

        private ParserClient(Uri baseUri)
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }
        #endregion

        #region Single
        /// <summary>Parse a grammar and use that grammar to parse a text, using the default rule.</summary>
        /// <param name="grammar">Grammar.</param>
        /// <param name="text">Text to parse with the grammar.</param>
        /// <returns>Parsing result of the text.</returns>
        public Task<ParsingResult> SingleParseAsync(string grammar, string text)
        {
            return SingleParseAsync(grammar, null, text);
        }

        /// <summary>Parse a grammar and use that grammar to parse a text.</summary>
        /// <param name="grammar">Grammar.</param>
        /// <param name="rule">Rule, within the grammar, to use to parse the text.</param>
        /// <param name="text">Text to parse with the grammar.</param>
        /// <returns>Parsing result of the text.</returns>
        public async Task<ParsingResult> SingleParseAsync(string grammar, string? rule, string text)
        {
            if (string.IsNullOrWhiteSpace(grammar))
            {
                throw new ArgumentNullException(nameof(grammar));
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            var input = new SingleParsingInput
            {
                Grammar = grammar,
                Rule = rule,
                Text = text
            };

            return await PostAsync<SingleParsingInput, ParsingResult>("v1/single", input);
        }
        #endregion

        #region Multiple
        /// <summary>Parse a grammar and use that grammar to parse a multiple texts.</summary>
        /// <param name="grammar">Grammar.</param>
        /// <param name="texts">Texts to parse with the grammar.</param>
        /// <returns>Parsing results of the texts:  one for each text.</returns>
        public Task<ParsingResult[]> MultipleParseAsync(string grammar, IEnumerable<string> texts)
        {
            return MultipleParseAsync(grammar, null, texts);
        }

        /// <summary>Parse a grammar and use that grammar to parse a multiple texts.</summary>
        /// <param name="grammar">Grammar.</param>
        /// <param name="rule">Rule, within the grammar, to use to parse the text.</param>
        /// <param name="texts">Texts to parse with the grammar.</param>
        /// <returns>Parsing results of the texts:  one for each text.</returns>
        public async Task<ParsingResult[]> MultipleParseAsync(string grammar, string? rule, IEnumerable<string> texts)
        {
            if (string.IsNullOrWhiteSpace(grammar))
            {
                throw new ArgumentNullException(nameof(grammar));
            }
            if (texts == null || !texts.Any())
            {
                throw new ArgumentNullException(nameof(texts));
            }

            var input = new MultipleParsingInput
            {
                Grammar = grammar,
                Rule = rule,
                Texts = texts.ToArray()
            };

            return await PostAsync<MultipleParsingInput, ParsingResult[]>("v1/multiple", input);
        }
        #endregion

        private async Task<Output> PostAsync<Input, Output>(string uriStem, Input input)
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri(_baseUri, new Uri(uriStem, UriKind.Relative));

                try
                {
                    var inputString = JsonConvert.SerializeObject(input);
                    var content = new StringContent(inputString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(uri, content);
                    var outputString = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var output = JsonConvert.DeserializeObject<Output>(outputString);

                        return output;
                    }
                    else
                    {
                        throw new ParsingException(outputString, (int)response.StatusCode);
                    }
                }
                catch (ParsingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Communication error with {uri}", ex);
                }
            }
        }
    }
}