using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class TaggedRule
    {
        public TaggedRule(IRule rule) : this(null, rule, true)
        {
        }

        public TaggedRule(string tag, IRule rule, bool doIncludeChildren = true)
        {
            if (tag == string.Empty)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            Tag = tag;
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            DoIncludeChildren = doIncludeChildren;
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

        public ImmutableList<NamedRuleMatch> AddFragment(
            ImmutableList<NamedRuleMatch> fragments,
            RuleMatch match)
        {
            return Tag == null
                ? fragments
                : fragments.Add(new NamedRuleMatch(Tag, FormatMatch(match)));
        }

        public override string ToString()
        {
            var colons = DoIncludeChildren ? ":" : "::";
            var rule = string.IsNullOrWhiteSpace(Rule.RuleName)
                    ? Rule.ToString()
                    : Rule.RuleName;

            return $"{Tag}{colons}({rule})";
        }

        private RuleMatch FormatMatch(RuleMatch match)
        {
            return DoIncludeChildren ? match : match.RemoveChildren();
        }
    }
}
