using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    public class NamedRuleMatch
    {
        public NamedRuleMatch(string name, RuleMatch match)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
            Match = match ?? throw new ArgumentNullException(nameof(match));
        }

        public string Name { get; }

        public RuleMatch Match { get; }
    }
}
