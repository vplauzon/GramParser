using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class TaggedRule
    {
        public TaggedRule(IRule rule)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public TaggedRule(string tag, IRule rule, bool doKeepGrandChildren)
        {
            if (tag == string.Empty)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            Tag = tag;
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
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

        public override string ToString()
        {
            var rule = string.IsNullOrWhiteSpace(Rule.RuleName)
                    ? Rule.ToString()
                    : Rule.RuleName;
            
            return $"{Tag}:({rule})";
        }
    }
}
