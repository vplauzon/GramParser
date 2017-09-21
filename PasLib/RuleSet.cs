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

        public RuleResult Match(string ruleName, SubString text, TracePolicy tracePolicy)
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
            var trialAccumulator = tracePolicy.CreateTrialAccumulator();
            var result = rule.Match(text, tracePolicy);

            tracePolicy.AddTrial(trialAccumulator, result);
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
                    return new RuleResult(
                        rule,
                        remainingText,
                        tracePolicy.ExtractTrials(trialAccumulator));
                }
                else
                {
                    var interleaveResult = _drainInterleavesRule.Match(remainingText, tracePolicy);

                    tracePolicy.AddTrial(trialAccumulator, interleaveResult);
                    if (interleaveResult.IsSuccess
                        && interleaveResult.Match.MatchLength == remainingText.Length)
                    {
                        return new RuleResult(
                            rule,
                            result.Match.IncreaseMatchLength(remainingText.Length),
                            tracePolicy.ExtractTrials(trialAccumulator));
                    }
                    else
                    {
                        return new RuleResult(
                            rule,
                            remainingText,
                            tracePolicy.ExtractTrials(trialAccumulator));
                    }
                }
            }
        }
    }
}