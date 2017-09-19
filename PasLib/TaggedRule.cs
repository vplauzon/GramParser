using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class TaggedRule
    {
        public TaggedRule(IRule rule) : this(null, rule, false)
        {
        }

        public TaggedRule(string tag, IRule rule) : this(tag, rule, false)
        {
        }

        public TaggedRule(string tag, IRule rule, bool doIncludeGrandChildren)
        {
            if (tag == string.Empty)
            {
                throw new ArgumentNullException(nameof(tag));
            }
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            Tag = tag;
            Rule = rule;
            DoIncludeGrandChildren = doIncludeGrandChildren;
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

        public bool DoIncludeGrandChildren { get; private set; }

        public override string ToString()
        {
            return "[" + Tag + "]" + Rule.ToString(); 
        }
    }
}
