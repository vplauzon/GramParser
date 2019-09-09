using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PasLib
{
    internal class SubstractRule : RuleBase
    {
        private readonly IRule _primary;
        private readonly IRule _excluded;

        public SubstractRule(
            string ruleName,
            IOutputExtractor outputExtractor,
            IRule primary,
            IRule excluded,
            bool? hasInterleave = null,
            bool? isRecursive = null,
            bool? hasChildrenDetails = null)
            : base(
                  ruleName,
                  outputExtractor,
                  hasInterleave,
                  isRecursive,
                  false,
                  hasChildrenDetails)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _excluded = excluded ?? throw new ArgumentNullException(nameof(excluded));
        }

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
                        new[] { primaryMatch });

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