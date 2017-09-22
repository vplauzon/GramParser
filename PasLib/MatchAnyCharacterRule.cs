using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal class MatchAnyCharacterRule : RuleBase
    {
        public MatchAnyCharacterRule(string ruleName) : base(ruleName)
        {
        }

        protected override RuleResult OnMatch(SubString text, int depth)
        {
            if (text.Length == 0)
            {
                return RuleResult.Failure(this, text);
            }
            else
            {
                return RuleResult.Success(
                    new RuleMatch(this, text.Take(1)));
            }
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (.)";
        }
    }
}
