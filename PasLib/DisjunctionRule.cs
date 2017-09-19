using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class DisjunctionRule : RuleBase
    {
        private readonly TaggedRule[] _rules;

        public DisjunctionRule(string ruleName, IRule interleave, IEnumerable<TaggedRule> rules)
            : base(ruleName, interleave)
        {
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = rules.ToArray();
        }

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            var shortestUnmatched = new SubString();

            foreach (var rule in _rules)
            {
                var subTrace = trace.Stack(rule.Rule, text);
                var result = subTrace == null
                    ? new RuleResult(text)
                    : rule.Rule.Match(text, subTrace);

                if (result.IsSuccess && result.Match.MatchLength != 0)
                {
                    if (rule.Tag == null)
                    {
                        var trimmedMatch = new RuleMatch(
                            RuleName,
                            result.Match.MatchLength,
                            text.Take(result.Match.MatchLength));

                        return new RuleResult(trimmedMatch);
                    }
                    else
                    {
                        var fragments = new Dictionary<string, RuleMatch>() { { rule.Tag, result.Match } };

                        return new RuleResult(new RuleMatch(RuleName, result.Match.MatchLength, fragments));
                    }
                }
                else if (result.IsFailure)
                {
                    if (shortestUnmatched.IsNull || result.Unmatched.Length < shortestUnmatched.Length)
                    {
                        shortestUnmatched = result.Unmatched;
                    }
                }
            }

            return shortestUnmatched.IsNull
                ? new RuleResult(text)
                : new RuleResult(shortestUnmatched);
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.Rule.ToString();

            return "<" + RuleName + "> (" + string.Join(" | ", rules) + ")";
        }
    }
}