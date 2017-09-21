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

        protected override RuleResult OnMatch(SubString text, TracePolicy tracePolicy)
        {
            if (text.HasContent)
            {
                var peek = text.First;

                if (peek >= First && peek <= Last)
                {
                    return new RuleResult(
                        this,
                        new RuleMatch(RuleName, 1, text.Take(1)),
                        tracePolicy.EmptyTrials);
                }
            }

            return new RuleResult(this, text, tracePolicy.EmptyTrials);
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (\"" + First + "\"..\"" + Last + "\")";
        }
    }
}