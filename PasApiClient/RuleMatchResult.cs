using System;
using System.Collections.Generic;
using System.Text;

namespace PasApiClient
{
    /// <summary>Parsing match of a grammar rule.</summary>
    public class RuleMatchResult
    {
        /// <summary>Name of the matched rule.</summary>
        public string Rule { get; set; }

        /// <summary>Text that was match with the rule.</summary>
        public string Text { get; set; }

        /// <summary>Children matches.</summary>
        public RuleMatchResult[] Children { get; set; }

        /// <summary>Named children matches.</summary>
        public IDictionary<string, RuleMatchResult> NamedChildren { get; set; }
    }
}