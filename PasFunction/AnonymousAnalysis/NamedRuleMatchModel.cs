using PasLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasFunction.AnonymousAnalysis
{
    internal class TaggedRuleMatchModel
    {
        public TaggedRuleMatchModel(TaggedRuleMatch taggedRuleMatch)
        {
            if (taggedRuleMatch == null)
            {
                throw new ArgumentNullException(nameof(taggedRuleMatch));
            }
            Tag = taggedRuleMatch.Tag;
            Match = new RuleMatchModel(taggedRuleMatch.Match);
        }

        public string Tag { get; }

        public RuleMatchModel Match { get; }
    }
}