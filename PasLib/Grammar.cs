using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PasLib
{
    public class Grammar
    {
        private const string DEFAULT_RULE_NAME = "main";
        private readonly IDictionary<string, IRule> _ruleMap;
        private readonly IRule _interleaveRule;

        internal Grammar(IEnumerable<IRule> rules, IRule interleaveRule)
        {
            if (rules == null || !rules.Any())
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _ruleMap = rules.ToDictionary(r => r.RuleName, r => r);
            _interleaveRule = interleaveRule == null
                ? null
                : new RepeatRule(null, interleaveRule, null, null, false, false);
        }

        public IEnumerable<IRule> Rules { get { return _ruleMap.Values; } }

        public IEnumerable<string> RuleNames
        {
            get
            {
                var names = from rule in _ruleMap.Values
                            select rule.RuleName;

                return names;
            }
        }

        public bool HasRule(string ruleName)
        {
            if(string.IsNullOrWhiteSpace(ruleName))
            {
                throw new ArgumentNullException(nameof(ruleName));
            }

            return RuleNames.Contains(ruleName);
        }

        public bool HasDefaultRule()
        {
            return HasRule(DEFAULT_RULE_NAME);
        }

        public RuleMatch Match(string ruleName, SubString text, int? depth = null)
        {
            if (text.IsNull)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (string.IsNullOrWhiteSpace(ruleName))
            {
                ruleName = DEFAULT_RULE_NAME;
            }
            if (!_ruleMap.ContainsKey(ruleName))
            {
                throw new ArgumentOutOfRangeException(nameof(ruleName), "Unknown rule");
            }

            var rule = _ruleMap[ruleName];
            var context = new ExplorerContext(text, _interleaveRule, depth);
            var matches = GetMatches(rule, context);
            var exactLengthMatches = from m in matches
                                     where m.LengthWithInterleaves == text.Length
                                     select m;
            //  Take the first available match (of right length)
            var match = exactLengthMatches.FirstOrDefault();

            return match;
        }

        private static IEnumerable<RuleMatch> GetMatches(
            IRule rule,
            ExplorerContext context)
        {
            var leftMatches = context.InvokeRule(rule);

            foreach (var m in leftMatches)
            {
                var newContext = context.MoveForward(m);
                var interleaveLength = newContext.MatchInterleave();
                var fullMatch = m.AddInterleaveLength(interleaveLength);

                yield return fullMatch;
            }
        }
    }
}