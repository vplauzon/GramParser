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

        public IEnumerable<RuleMatch> Match(ExplorerContext context)
        {
            var newContext = context.TryMoveDown(this);

            return newContext == null ? EmptyMatch : OnMatch(newContext);
        }

        protected abstract IEnumerable<RuleMatch> OnMatch(ExplorerContext context);
    }
}