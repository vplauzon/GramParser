﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class RangeRule : RuleBase
    {
        public RangeRule(
            string ruleName,
            IRuleOutput ruleOutput,
            char first,
            char last)
            : base(ruleName, ruleOutput, false, true, true)
        {
            if (last < first)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(last),
                    "Must be greater or equal to first");
            }
            First = first;
            Last = last;
        }

        public char First { get; }

        public char Last { get; }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var text = context.Text;

            if (text.HasContent)
            {
                var peek = text.First();

                if (peek >= First && peek <= Last)
                {
                    var matchText = text.Take(1);
                    var match = new RuleMatch(
                        this,
                        matchText,
                        () => RuleOutput.ComputeOutput(matchText, matchText));

                    return new[] { match };
                }
            }

            return RuleMatch.EmptyMatch;
        }

        public override string ToString()
        {
            return ToStringRuleName() + $" (\"{First}\"..\"{Last}\")";
        }
    }
}