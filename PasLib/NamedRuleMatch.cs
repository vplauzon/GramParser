using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    public class TaggedRuleMatch
    {
        public TaggedRuleMatch(string tag, RuleMatch match)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentNullException(nameof(tag));
            }
            Tag = tag;
            Match = match ?? throw new ArgumentNullException(nameof(match));
        }

        public string Tag { get; }

        public RuleMatch Match { get; }
    }
}
