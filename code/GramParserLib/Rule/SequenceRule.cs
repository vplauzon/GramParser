﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib.Rule
{
    internal class SequenceRule : RuleBase
    {
        private readonly TaggedRuleCollection _rules;

        public SequenceRule(
            string? ruleName,
            IRuleOutput? ruleOutput,
            IEnumerable<TaggedRule> rules,
            bool? hasInterleave = null,
            bool? isCaseSensitive = null)
            : base(
                  ruleName,
                  ruleOutput,
                  hasInterleave,
                  isCaseSensitive)
        {
            if (rules == null || !rules.Any())
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = new TaggedRuleCollection(rules);
        }

        public override bool IsTerminalRule => false;

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            if (RuleName == "integer")
            {
                int a = 3;
                ++a;
            }
            if (RuleName == "add")
            {
                int a = 3;
                ++a;
            }
            return RecurseMatch(
                _rules,
                context,
                context.Text,
                0,
                ImmutableList<(TaggedRule rule, RuleMatch match)>.Empty);
        }

        #region object methods
        public override string ToString()
        {
            var rules = from t in _rules
                        select t.ToString();

            return ToStringRuleName()
                + $"({string.Join(" ", rules)})";
        }
        #endregion

        private IEnumerable<RuleMatch> RecurseMatch(
            IEnumerable<TaggedRule> rules,
            ExplorerContext context,
            SubString originalText,
            int totalMatchLength,
            ImmutableList<(TaggedRule rule, RuleMatch match)> matches)
        {
            var currentRule = rules.First();
            var remainingRules = rules.Skip(1);
            var potentials = context.InvokeRule(currentRule.Rule);

            foreach (var match in potentials)
            {
                var newTotalMatchLength = totalMatchLength + match.LengthWithInterleaves;
                var newMatches = matches.Add((currentRule, match));

                if (remainingRules.Any())
                {   //  Recurse
                    var newContext = context.MoveForward(match);
                    var downstreamMatches = RecurseMatch(
                        remainingRules,
                        newContext,
                        originalText,
                        newTotalMatchLength,
                        newMatches);

                    foreach (var m in downstreamMatches)
                    {
                        yield return m;
                    }
                }   //  End recursion
                else
                {
                    var matchText = originalText.Take(newTotalMatchLength);

                    yield return new RuleMatch(
                        this,
                        matchText,
                        () => ComputeOutput(matchText, newMatches));
                }
            }
        }

        private object? ComputeOutput(
            SubString text,
            ImmutableList<(TaggedRule rule, RuleMatch match)> subMatches)
        {
            if (!_rules.DoAllNotHaveNames)
            {
                var components = from m in subMatches
                                 where m.rule.HasTag
                                 select new
                                 {
                                     Name = m.rule.Tag,
                                     Output = m.match.ComputeOutput()
                                 };

                return RuleOutput.ComputeOutput(
                    text,
                    new Lazy<object?>(() => components.ToImmutableDictionary(c => c.Name, c => c.Output)));
            }
            else
            {
                Func<object> outputFactory = () => subMatches
                    .Select(m => m.match.ComputeOutput())
                    .ToImmutableArray();

                return RuleOutput.ComputeOutput(text, new Lazy<object?>(outputFactory));
            }
        }
    }
}