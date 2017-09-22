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

        protected override IEnumerable<RuleMatch> OnMatch(SubString text, int depth)
        {
            foreach (var rule in _rules)
            {
                var matches = rule.Rule.Match(text, depth - 1);

                foreach (var m in matches)
                {
                    if (rule.HasTag)
                    {
                        var fragments = new Dictionary<string, RuleMatch>() { { rule.Tag, m } };

                        yield return new RuleMatch(
                            this,
                            m.Content,
                            fragments);
                    }
                    else
                    {
                        yield return m.ChangeRule(this);
                    }
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