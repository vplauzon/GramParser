using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class RangeRule : RuleBase
    {
        public RangeRule(string ruleName, char first, char last) : base(ruleName)
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

        public char First { get; private set; }

        public char Last { get; private set; }

        protected override RuleResult OnMatch(SubString text, int depth)
        {
            if (text.HasContent)
            {
                var peek = text.First;

                if (peek >= First && peek <= Last)
                {
                    return RuleResult.Success(new RuleMatch(this, text.Take(1)));
                }
            }

            return RuleResult.Failure(this, text);
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (\"" + First + "\"..\"" + Last + "\")";
        }
    }
}