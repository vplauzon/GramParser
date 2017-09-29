using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class RangeRule : RuleBase
    {
        public RangeRule(string ruleName, char first, char last) : base(
            ruleName,
            false,
            true,
            true)
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
                var peek = text.First;

                if (peek >= First && peek <= Last)
                {
                    return new[]
                    {
                        new RuleMatch(this, text.Take(1))
                    };
                }
            }

            return EmptyMatch;
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (\"" + First + "\"..\"" + Last + "\")";
        }
    }
}