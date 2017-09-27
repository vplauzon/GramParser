using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class DisjunctionRule : RuleBase
    {
        private readonly TaggedRule[] _rules;

        public DisjunctionRule(string ruleName, IEnumerable<TaggedRule> rules)
            : base(ruleName)
        {
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = rules.ToArray();
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            foreach (var rule in _rules)
            {
                var matches = rule.Rule.Match(context);

                foreach (var m in matches)
                {
                    var fragments = rule.AddFragment(TaggedRule.EMPTY_FRAGMENTS, m);

                    yield return new RuleMatch(this, m.Text, fragments);
                }
            }
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.Rule.ToString();

            return "<" + RuleName + "> (" + string.Join(" | ", rules) + ")";
        }
    }
}