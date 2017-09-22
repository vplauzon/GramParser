using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class SequenceRule : RuleBase
    {
        private readonly TaggedRule[] _rules;

        public SequenceRule(string ruleName, IEnumerable<TaggedRule> rules)
            : base(ruleName)
        {
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = rules.ToArray();
        }

        protected override RuleResult OnMatch(SubString text, int depth)
        {
            var fragments = new Dictionary<string, RuleMatch>();
            var totalMatchLength = 0;
            var originalText = text;

            foreach (var rule in _rules)
            {
                var result = rule.Rule.Match(text, depth - 1);

                if (result.IsSuccess)
                {
                    if (rule.Tag != null)
                    {
                        fragments[rule.Tag] = result.Match;
                    }
                    text = text.Skip(result.Match.Content.Length);

                    totalMatchLength += result.Match.Content.Length;
                }
                else
                {
                    return result;
                }
            }

            var content = originalText.Take(totalMatchLength);

            return fragments.Any()
                ? RuleResult.Success(new RuleMatch(this, content, fragments))
                : RuleResult.Success(new RuleMatch(this, content));
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.Rule.ToString();

            return "<" + RuleName + "> (" + string.Join(" ", rules) + ")";
        }
    }
}