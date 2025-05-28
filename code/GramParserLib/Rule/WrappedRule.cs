using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    /// <summary>
    /// A rule that actually reference another rule, 
    /// e.g. rule A = B;
    /// </summary>
    internal class WrappedRule : IRule
    {
        private readonly string? _ruleName;

        public WrappedRule(
            string? ruleName,
            IRuleOutput? ruleOutput,
            IRule referencedRule)
        {
            _ruleName = ruleName;
            ReferencedRule = referencedRule;
        }

        #region IRuleProperties
        public bool? HasInterleave => ReferencedRule.HasInterleave;

        public bool IsTerminalRule => false;

        public bool? IsCaseSensitive => ReferencedRule.IsCaseSensitive;
        #endregion

        public string? RuleName => _ruleName ?? ReferencedRule.RuleName;

        public IRule ReferencedRule { get; }

        public IEnumerable<RuleMatch> Match(ExplorerContext context)
        {
            return ReferencedRule.Match(context);
        }
    }
}