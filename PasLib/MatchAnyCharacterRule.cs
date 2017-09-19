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

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            if (text.Length == 0)
            {
                return new RuleResult(text);
            }
            else
            {
                return new RuleResult(new RuleMatch(RuleName, 1, text.Take(1)));
            }
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (.)";
        }
    }
}
