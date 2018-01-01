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
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            MatchSelectionState = MatchSelection.Unspecified;
        }

        public TaggedRule(string tag, IRule rule, bool doKeepGrandChildren)
        {
            if (tag == string.Empty)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            Tag = tag;
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            MatchSelectionState = doKeepGrandChildren
                ? MatchSelection.GrandChildren
                : MatchSelection.ChildrenOnly;
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

        public MatchSelection MatchSelectionState { get; }

        public override string ToString()
        {
            var rule = string.IsNullOrWhiteSpace(Rule.RuleName)
                    ? Rule.ToString()
                    : Rule.RuleName;

            if (MatchSelectionState == MatchSelection.Unspecified)
            {
                return $"({rule})";
            }
            else
            {
                var colons = MatchSelectionState == MatchSelection.GrandChildren
                    ? ":"
                    : "::";

                return $"{Tag}{colons}({rule})";
            }
        }
    }
}
