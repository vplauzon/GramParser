using GramParserLib.Output;
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
        private readonly IRuleOutput _ruleOutput;

        public WrappedRule(string? ruleName, IRuleOutput? ruleOutput, IRule referencedRule)
        {
            _ruleName = ruleName;
            _ruleOutput = ruleOutput ?? DefaultOutput.Instance;
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
            foreach (var match in ReferencedRule.Match(context))
            {
                yield return new RuleMatch(
                    this,
                    match.Text,
                    () => _ruleOutput.ComputeOutput(
                        match.Text,
                        new Lazy<object?>(() => match.ComputeOutput())));
            }
        }
    }
}