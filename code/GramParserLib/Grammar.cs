using GramParserLib.Rule;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GramParserLib
{
    public class Grammar
    {
        private const string DEFAULT_RULE_NAME = "main";
        private readonly IImmutableDictionary<string, IRule> _ruleMap;
        private readonly IRule _interleaveRule;

        internal Grammar(IDictionary<string, IRule> ruleMap, IRule interleaveRule)
        {
            if (ruleMap == null || !ruleMap.Any())
            {
                throw new ArgumentNullException(nameof(ruleMap));
            }

            _ruleMap = ImmutableDictionary<string, IRule>.Empty.AddRange(ruleMap);
            _interleaveRule = interleaveRule;
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
            if (string.IsNullOrWhiteSpace(ruleName))
            {
                throw new ArgumentNullException(nameof(ruleName));
            }

            return RuleNames.Contains(ruleName);
        }

        public bool HasDefaultRule()
        {
            return HasRule(DEFAULT_RULE_NAME);
        }

        public RuleMatch? Match(string? ruleName, SubString text, int? maxDepth = null)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
            {
                ruleName = DEFAULT_RULE_NAME;
            }
            if (!_ruleMap.ContainsKey(ruleName))
            {
                throw new ParsingException($"Unknown rule to match:  '{ruleName}'");
            }

            var rule = _ruleMap[ruleName];
            var context = new ExplorerContext(text, _interleaveRule, maxDepth);
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