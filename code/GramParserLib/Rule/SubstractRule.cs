using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib.Rule
{
    internal class SubstractRule : RuleBase
    {
        private readonly IRule _primary;
        private readonly IRule _excluded;

        public SubstractRule(
            string ruleName,
            IRuleOutput ruleOutput,
            IRule primary,
            IRule excluded,
            bool? hasInterleave = null,
            bool? isRecursive = null)
            : base(
                  ruleName,
                  ruleOutput,
                  hasInterleave,
                  isRecursive)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _excluded = excluded ?? throw new ArgumentNullException(nameof(excluded));
        }

        public override bool IsTerminalRule => false;

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var primaryMatches = context.InvokeRule(_primary);

            foreach (var primaryMatch in primaryMatches)
            {
                var primaryText = primaryMatch.Text;
                var excludingContext = context.SubContext(primaryText.Length);
                var excludedMatches = excludingContext.InvokeRule(_excluded);
                var excludedExactLength = from ex in excludedMatches
                                          where ex.Text.Length == primaryText.Length
                                          select ex;

                if (!excludedExactLength.Any())
                {
                    var match = new RuleMatch(
                        this,
                        primaryText,
                        () => RuleOutput.ComputeOutput(
                            primaryText,
                            new Lazy<object>(() => primaryMatch.ComputeOutput())));

                    yield return match;
                }
            }
        }

        public override string ToString()
        {
            return ToStringRuleName()
                + $"({_primary} - {ToString(_excluded)})";
        }
    }
}