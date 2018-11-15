using System;
using System.Collections.Generic;
using System.Text;

namespace PasApiClient
{
    /// <summary>Result of parsing.</summary>
    public class ParsingResult
    {
        /// <summary>Version of the API.</summary>
        public string ApiVersion { get; set; }

        /// <summary>Is match successful:  <c>true</c> iif a match was found.</summary>
        public bool IsMatch { get; set; }

        /// <summary>Match found.</summary>
        public RuleMatchResult RuleMatch { get; set; }
    }
}