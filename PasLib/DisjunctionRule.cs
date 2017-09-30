using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class DisjunctionRule : RuleBase
    {
        private readonly TaggedRule[] _rules;

        public DisjunctionRule(
            string ruleName,
            IEnumerable<TaggedRule> rules,
            bool? hasInterleave = null,
            bool? isRecursive = null)
            : base(ruleName, hasInterleave, isRecursive, false)
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
                var matches = context.InvokeRule(rule.Rule);

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
                        select ToString(t.Rule);

            return "<" + RuleName + "> (" + string.Join(" | ", rules) + ")";
        }
    }
}