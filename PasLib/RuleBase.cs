using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal abstract class RuleBase : IRule
    {
        protected RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        protected RuleBase(
            string ruleName,
            bool? hasInterleave,
            bool? isRecursive,
            bool isTerminalRule)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            RuleName = ruleName;
            HasInterleave = hasInterleave;
            IsRecursive = isRecursive;
            IsTerminalRule = isTerminalRule;
        }

        #region IRuleProperties
        public bool? HasInterleave { get; }

        public bool? IsRecursive { get; }

        public bool IsTerminalRule { get; }
        #endregion

        public string RuleName { get; private set; }

        public IEnumerable<RuleMatch> Match(ExplorerContext context)
        {
            var newContext = context.TryMoveDown(this);

            return newContext == null ? EmptyMatch : OnMatch(newContext);
        }

        protected abstract IEnumerable<RuleMatch> OnMatch(ExplorerContext context);
    }
}