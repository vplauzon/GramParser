using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class TaggedRule
    {
        public TaggedRule(IRule rule)
            : this(null, rule)
        {
        }

        public TaggedRule(
            string tag,
            IRule rule,
            bool doIncludeChildren = true,
            MatchSelection matchSelectionState = MatchSelection.Unspecified)
        {
            if (tag == string.Empty)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            Tag = tag;
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            DoIncludeChildren = doIncludeChildren;
            MatchSelectionState = HasTag
                ? MatchSelection.GrandChildren
                : MatchSelection.Unspecified;
        }

        public static IEnumerable<TaggedRule> FromRules(params IRule[] rules)
        {
            return FromRules(rules as IEnumerable<IRule>);
        }

        public static IEnumerable<TaggedRule> FromRules(IEnumerable<IRule> rules)
        {
            var tagged = from r in rules
                         select new TaggedRule(r);

            return tagged;
        }

        public bool HasTag { get { return !string.IsNullOrWhiteSpace(Tag); } }

        public string Tag { get; private set; }

        public IRule Rule { get; private set; }

        public bool DoIncludeChildren { get; }

        public MatchSelection MatchSelectionState { get; }

        public override string ToString()
        {
            var colons = DoIncludeChildren ? ":" : "::";
            var rule = string.IsNullOrWhiteSpace(Rule.RuleName)
                    ? Rule.ToString()
                    : Rule.RuleName;

            return $"{Tag}{colons}({rule})";
        }
    }
}
