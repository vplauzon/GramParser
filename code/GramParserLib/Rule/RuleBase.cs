using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal abstract class RuleBase : IRule
    {
        protected RuleBase(
            string ruleName,
            IRuleOutput ruleOutput,
            bool? hasInterleave,
            bool? isRecursive,
            bool isTerminalRule,
            bool? hasChildrenDetails)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            RuleName = ruleName;
            RuleOutput = ruleOutput;
            HasInterleave = hasInterleave;
            IsRecursive = isRecursive;
            IsTerminalRule = isTerminalRule;
            HasChildrenDetails = hasChildrenDetails;
        }

        #region IRuleProperties
        public bool? HasInterleave { get; }

        public bool? IsRecursive { get; }

        public bool IsTerminalRule { get; }

        public bool? HasChildrenDetails { get; }
        #endregion

        public string RuleName { get; private set; }

        public IEnumerable<RuleMatch> Match(ExplorerContext context)
        {
            return OnMatch(context);
        }

        protected IRuleOutput RuleOutput { get; }

        protected abstract IEnumerable<RuleMatch> OnMatch(ExplorerContext context);

        protected string ToString(IRule rule)
        {
            return string.IsNullOrWhiteSpace(rule.RuleName)
                ? rule.ToString()
                : rule.RuleName;
        }

        protected string ToStringRuleName()
        {
            return (string.IsNullOrWhiteSpace(RuleName) ? "" : $"<{RuleName}>");
        }
    }
}