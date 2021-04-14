using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class WrappedRule : RuleBase
    {
        private readonly IRule _referencedRule;

        public WrappedRule(
            string? ruleName,
            IRuleOutput? ruleOutput,
            IRule referencedRule,
            bool? hasInterleave = null,
            bool? isRecursive = null,
            bool? isCaseSensitive = null)
            : base(
                  ruleName,
                  ruleOutput,
                  hasInterleave,
                  isRecursive,
                  isCaseSensitive)
        {
            _referencedRule = referencedRule;
        }

        public override bool IsTerminalRule => false;

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var matches = _referencedRule.Match(context);
            var wrapMatches = from m in matches
                              select new RuleMatch(
                                  this,
                                  m.Text,
                                  () => RuleOutput.ComputeOutput(
                                      m.Text,
                                      new Lazy<object?>(() => m.ComputeOutput())));
            
            return wrapMatches;
        }
    }
}