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

        public RuleMatch Match(string ruleName, SubString text, int? depth = null)
        {
            if (text.IsNull)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (!_ruleMap.ContainsKey(ruleName))
            {
                throw new ArgumentOutOfRangeException(nameof(ruleName), "Unknown rule");
            }

            var actualDepth = depth ?? DEFAULT_MAX_DEPTH;
            var rule = _ruleMap[ruleName];
            var matches = rule.Match(text, actualDepth);
            var exactLengthMatches = from m in matches
                                     where m.Content.Length == text.Length
                                     select m;
            //  Take the first available match (of right length)
            var match = exactLengthMatches.FirstOrDefault();

            return match;
        }
    }
}