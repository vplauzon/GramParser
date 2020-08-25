using System;
using System.Collections.Generic;
using System.Text;

namespace PasApiClient
{
    /// <summary>Default implementation with <see cref="object"/> as output.</summary>
    public class ParsingResult : ParsingResult<object>
    {
    }

    /// <summary>Result of parsing.</summary>
    public class ParsingResult<OUTPUT> where OUTPUT : class
    {
        /// <summary>Version of the API.</summary>
        public string? ApiVersion { get; set; }

        /// <summary>Is match successful:  <c>true</c> iif a match was found.</summary>
        public bool IsMatch { get; set; }

        /// <summary>Match found.</summary>
        public RuleMatchResult? RuleMatch { get; set; }

        /// <summary>Output of the rule.</summary>
        public OUTPUT? Output{ get; set; }
    }
}