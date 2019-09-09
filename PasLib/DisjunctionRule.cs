using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PasLib
{
    internal class DisjunctionRule : RuleBase
    {
        private readonly TaggedRuleCollection _rules;

        public DisjunctionRule(
            string ruleName,
            IOutputExtractor outputExtractor,
            IEnumerable<TaggedRule> rules,
            bool? hasInterleave = null,
            bool? isRecursive = null,
            bool? hasChildrenDetails = null)
            : base(
                  ruleName,
                  outputExtractor,
                  hasInterleave,
                  isRecursive,
                  false,
                  hasChildrenDetails)
        {
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = new TaggedRuleCollection(rules);
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            foreach (var rule in _rules)
            {
                var potentials = context.InvokeRule(rule.Rule);

                foreach (var m in potentials)
                {
                    var subMatches = _rules.AddMatch(
                        ImmutableList<(TaggedRule, RuleMatch)>.Empty,
                        rule,
                        m);

                    yield return _rules.CreateMatch(
                        this,
                        m.Text,
                        subMatches);
                }
            }
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.ToString();
            var join = string.Join(" | ", rules);

            return ToStringRuleName() + $"({join})";
        }
    }
}