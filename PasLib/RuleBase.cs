using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal abstract class RuleBase : IRule
    {
        protected RuleBase(string ruleName)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            RuleName = ruleName;
        }

        public string RuleName { get; private set; }

        public RuleResult Match(SubString text, int depth)
        {
            if (depth <= 0)
            {
                throw new InvalidOperationException("Too much recursion");
            }
            else
            {
                return OnMatch(text, depth);
            }
        }

        protected abstract RuleResult OnMatch(SubString text, int depth);
    }
}