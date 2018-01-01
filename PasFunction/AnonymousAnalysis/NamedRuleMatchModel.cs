using PasLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasFunction.AnonymousAnalysis
{
    internal class NamedRuleMatchModel
    {
        public NamedRuleMatchModel(NamedRuleMatch namedRuleMatch)
        {
            if (namedRuleMatch == null)
            {
                throw new ArgumentNullException(nameof(namedRuleMatch));
            }
            Name = namedRuleMatch.Name;
            Match = new RuleMatchModel(namedRuleMatch.Match);
        }

        public string Name { get; }

        public RuleMatchModel Match { get; }
    }
}