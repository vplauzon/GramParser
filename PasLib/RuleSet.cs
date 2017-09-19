using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class RuleSet
    {
        private readonly IDictionary<string, IRule> _ruleMap;
        private readonly IRule _drainInterleavesRule;

        public RuleSet(IRule interleave, IEnumerable<IRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            Interleave = interleave;
            _drainInterleavesRule = Interleave == null
                ? MatchNoneRule.Instance
                : new RepeatRule(null, null, Interleave, null, null, false, false);
            _ruleMap = rules.ToDictionary(r => r.RuleName, r => r);
        }

        public IRule Interleave { get; private set; }

        public IEnumerable<IRule> Rules { get { return _ruleMap.Values; } }

        public RuleResult Match(string ruleName, SubString text)
        {
            if (text.IsNull)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (!_ruleMap.ContainsKey(ruleName))
            {
                throw new ArgumentOutOfRangeException(nameof(ruleName), "Unknown rule");
            }

            var rule = _ruleMap[ruleName];
            var result = rule.Match(text, new RuleTrace());

            if (result.IsFailure || result.Match.MatchLength == text.Length)
            {
                return result;
            }
            else
            {
                //  Make sure we can match the entire text
                var remainingText = text.Skip(result.Match.MatchLength);

                if (Interleave == null)
                {   //  Failure
                    return new RuleResult(remainingText);
                }
                else
                {
                    var interleaveResult = _drainInterleavesRule.Match(remainingText, new RuleTrace());

                    if (interleaveResult.IsSuccess && interleaveResult.Match.MatchLength == remainingText.Length)
                    {
                        return new RuleResult(result.Match.IncreaseMatchLength(remainingText.Length));
                    }
                    else
                    {
                        return new RuleResult(remainingText);
                    }
                }
            }
        }
    }
}