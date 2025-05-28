using GramParserLib.Output;
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
            string? ruleName,
            IRuleOutput? ruleOutput,
            bool? hasInterleave,
            bool? isCaseSensitive)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            RuleName = ruleName;
            RuleOutput = ruleOutput ?? DefaultOutput.Instance;
            HasInterleave = hasInterleave;
            IsCaseSensitive = isCaseSensitive;
        }

        #region IRuleProperties
        public bool? HasInterleave { get; }

        public abstract bool IsTerminalRule { get; }
        
        public bool? IsCaseSensitive { get; }
        #endregion

        public string? RuleName { get; private set; }

        public IEnumerable<RuleMatch> Match(ExplorerContext context)
        {
            return OnMatch(context);
        }

        protected IRuleOutput RuleOutput { get; }

        protected abstract IEnumerable<RuleMatch> OnMatch(ExplorerContext context);

        protected string ToString(IRule rule)
        {
            return string.IsNullOrWhiteSpace(rule.RuleName)
                ? rule.ToString()!
                : rule.RuleName;
        }

        protected string ToStringRuleName()
        {
            return (string.IsNullOrWhiteSpace(RuleName) ? "" : $"<{RuleName}>");
        }
    }
}