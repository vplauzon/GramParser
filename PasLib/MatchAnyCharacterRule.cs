using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal class MatchAnyCharacterRule : RuleBase
    {
        public MatchAnyCharacterRule(string ruleName, IRule interleave) : base(ruleName, interleave)
        {
        }

        protected override RuleResult OnMatch(SubString text, TracePolicy tracePolicy)
        {
            if (text.Length == 0)
            {
                return new RuleResult(this, text, tracePolicy.EmptyTrials);
            }
            else
            {
                return new RuleResult(
                    this,
                    new RuleMatch(RuleName, 1, text.Take(1)),
                    tracePolicy.EmptyTrials);
            }
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (.)";
        }
    }
}
