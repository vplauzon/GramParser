using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class WrappedRule : IRule
    {
        private readonly string? _ruleName;
        private readonly IRule _referencedRule;

        public WrappedRule(
            string? ruleName,
            IRuleOutput? ruleOutput,
            IRule referencedRule)
        {
            _ruleName = ruleName;
            _referencedRule = referencedRule;
        }

        #region IRuleProperties
        public bool? HasInterleave => _referencedRule.HasInterleave;

        public bool? IsRecursive => _referencedRule.IsRecursive;

        public bool IsTerminalRule => false;

        public bool? IsCaseSensitive => _referencedRule.IsCaseSensitive;
        #endregion

        public string? RuleName => _ruleName;

        public IEnumerable<RuleMatch> Match(ExplorerContext context)
        {
            return _referencedRule.Match(context);
        }
    }
}