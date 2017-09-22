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

        public static int DEFAULT_MAX_DEPTH { get => 10; }

        public RuleSet(IRule interleave, IEnumerable<IRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            Interleave = interleave;
            _drainInterleavesRule = Interleave == null
                ? MatchNoneRule.Instance
                : new RepeatRule(null, Interleave, null, null, false, false);
            _ruleMap = rules.ToDictionary(r => r.RuleName, r => r);
        }

        public IRule Interleave { get; private set; }

        public IEnumerable<IRule> Rules { get { return _ruleMap.Values; } }

        public RuleResult Match(string ruleName, SubString text, int? depth = null)
        {
            var actualDepth = depth ?? DEFAULT_MAX_DEPTH;

            if (text.IsNull)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (!_ruleMap.ContainsKey(ruleName))
            {
                throw new ArgumentOutOfRangeException(nameof(ruleName), "Unknown rule");
            }

            var rule = _ruleMap[ruleName];
            var result = rule.Match(text, actualDepth);

            if (result.IsFailure || !result.Match.Content.HasContent)
            {
                return result;
            }
            else
            {
                //  Make sure we can match the entire text
                var remainingText = text.Skip(result.Match.Content.Length);

                if (Interleave == null)
                {
                    return RuleResult.Failure(rule, remainingText);
                }
                else
                {
                    var interleaveResult = _drainInterleavesRule.Match(remainingText, actualDepth);

                    if (interleaveResult.IsSuccess
                        && interleaveResult.Match.Content.Length == remainingText.Length)
                    {
                        return RuleResult.Success(result.Match);
                    }
                    else
                    {
                        return RuleResult.Failure(Interleave, remainingText);
                    }
                }
            }
        }
    }
}