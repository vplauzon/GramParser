using System;
using System.Collections.Generic;
using System.Text;

namespace PasApiClient
{
    public class RuleMatchResult
    {
        public string Rule { get; set; }

        public string Text { get; set; }

        public RuleMatchResult[] Children { get; set; }

        public NamedRuleMatchResult[] NamedChildren { get; set; }
    }
}