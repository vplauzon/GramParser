using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class RangeRule : RuleBase
    {
        public RangeRule(string ruleName, IRule interleave, char first, char last) : base(ruleName, interleave)
        {
            if (last < first)
            {
                throw new ArgumentOutOfRangeException(nameof(last), "Must be greater or equal to first");
            }
            First = first;
            Last = last;
        }

        public char First { get; private set; }

        public char Last { get; private set; }

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            if (text.HasContent)
            {
                var peek = text.First;

                if (peek >= First && peek <= Last)
                {
                    return new RuleResult(new RuleMatch(RuleName, 1, text.Take(1)));
                }
            }

            return new RuleResult(text);
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (\"" + First + "\"..\"" + Last + "\")";
        }
    }
}