using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib.Rule
{
    internal class DisjunctionRule : RuleBase
    {
        private readonly TaggedRuleCollection _rules;

        public DisjunctionRule(
            string ruleName,
            IRuleOutput ruleOutput,
            IEnumerable<TaggedRule> rules,
            bool? hasInterleave = null,
            bool? isRecursive = null,
            bool? hasChildrenDetails = null)
            : base(
                  ruleName,
                  ruleOutput,
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
                    if (_rules.HasNames)
                    {
                        yield return new RuleMatch(
                            this,
                            m.Text,
                            () => RuleOutput.ComputeOutput(m.Text, m.ComputeOutput()));
                    }
                    else
                    {
                        yield return new RuleMatch(
                            this,
                            m.Text,
                            () => RuleOutput.ComputeOutput(
                                m.Text,
                                MakeMap(rule.Tag, m.ComputeOutput())));
                    }
                }
            }
        }

        private static IDictionary<string, object> MakeMap(string tag, object output)
        {
            var dictionary = new Dictionary<string, object>(new[]
            {
                KeyValuePair.Create(tag, output)
            });

            return dictionary;
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