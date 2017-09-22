using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal abstract class RuleBase : IRule
    {
        protected RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        protected RuleBase(string ruleName)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            RuleName = ruleName;
        }

        public string RuleName { get; private set; }

        public IEnumerable<RuleMatch> Match(SubString text, int depth)
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

        protected abstract IEnumerable<RuleMatch> OnMatch(SubString text, int depth);
    }
}