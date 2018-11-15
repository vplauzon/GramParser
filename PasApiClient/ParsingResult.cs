using System;
using System.Collections.Generic;
using System.Text;

namespace PasApiClient
{
    public class ParsingResult
    {
        public string ApiVersion { get; set; }

        public bool IsMatch { get; set; }

        public RuleMatchResult RuleMatch { get; set; }
    }
}