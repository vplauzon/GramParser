﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib.Rule
{
    internal class DisjunctionRule : RuleBase
    {
        private readonly TaggedRuleCollection _rules;

        public DisjunctionRule(
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
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = new TaggedRuleCollection(rules);
            if (!_rules.DoAllHaveNames && !_rules.DoAllNotHaveNames)
            {
                throw new ParsingException(
                    "Can't have both named & unnamed rule match in one disjunction rule");
            }
        }

        public override bool IsTerminalRule => false;

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            foreach (var rule in _rules)
            {
                var potentials = context.InvokeRule(rule.Rule);

                foreach (var m in potentials)
                {
                    if (_rules.DoAllHaveNames)
                    {
                        yield return new RuleMatch(
                            this,
                            m.Text,
                            () => RuleOutput.ComputeOutput(
                                m.Text,
                                new Lazy<object?>(() => MakeMap(rule.Tag, m.ComputeOutput()))));
                    }
                    else
                    {
                        yield return new RuleMatch(
                            this,
                            m.Text,
                            () => RuleOutput.ComputeOutput(
                                m.Text,
                                new Lazy<object?>(() => m.ComputeOutput())));
                    }
                }
            }
        }

        private static IImmutableDictionary<string, object?> MakeMap(string? tag, object? output)
        {
            if (tag == null)
            {
                throw new NotSupportedException("Tag can't be null here");
            }

            var dictionary = ImmutableDictionary<string, object?>
                .Empty
                .Add(tag, output);

            return dictionary;
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.ToString();
            var join = string.Join(" | ", rules);

            return ToStringRuleName() + $"({join})";
        }
    }
}