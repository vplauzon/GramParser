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

        protected override RuleResult OnMatch(SubString text, int depth)
        {
            var shortestUnmatched = new SubString();

            foreach (var rule in _rules)
            {
                var result = rule.Rule.Match(text, depth - 1);

                if (result.IsSuccess)
                {
                    if (rule.Tag == null)
                    {
                        return RuleResult.Success(result.Match.ChangeRule(this));
                    }
                    else
                    {
                        var fragments = new Dictionary<string, RuleMatch>() {
                            { rule.Tag, result.Match } };

                        return RuleResult.Success(
                            new RuleMatch(this, result.Match.Content, fragments));
                    }
                }
                else
                {
                    if (shortestUnmatched.IsNull
                        || result.Unmatched.Length < shortestUnmatched.Length)
                    {
                        shortestUnmatched = result.Unmatched;
                    }
                }
            }

            return shortestUnmatched.IsNull
                ? RuleResult.Failure(this, text)
                : RuleResult.Failure(this, shortestUnmatched);
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.Rule.ToString();

            return "<" + RuleName + "> (" + string.Join(" | ", rules) + ")";
        }
    }
}