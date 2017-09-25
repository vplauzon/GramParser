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

        public static int DEFAULT_MAX_DEPTH { get => 40; }

        public RuleSet(IEnumerable<IRule> rules)
        {
            if (rules == null || !rules.Any())
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _ruleMap = rules.ToDictionary(r => r.RuleName, r => r);
        }

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
                                     where m.Text.Length == text.Length
                                     select m;
            //  Take the first available match (of right length)
            var match = exactLengthMatches.FirstOrDefault();

            return match;
        }
    }
}