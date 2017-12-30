using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PasLib
{
    internal class SubstractRule : RuleBase
    {
        private readonly TaggedRule _primary;
        private readonly IRule _excluded;

        public SubstractRule(
            string ruleName,
            TaggedRule primary,
            IRule excluded,
            bool? isRecursive = null)
            : base(ruleName, false, isRecursive, false)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _excluded = excluded ?? throw new ArgumentNullException(nameof(excluded));
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var primaryMatches = context.InvokeRule(_primary.Rule);

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
                    if (_primary.HasTag)
                    {
                        var fragment = new TaggedRuleMatch(_primary.Tag, primaryMatch);

                        yield return new RuleMatch(
                            this,
                            primaryMatch.Text,
                            ImmutableList<TaggedRuleMatch>.Empty.Add(fragment));
                    }
                    else
                    {
                        yield return primaryMatch.ChangeRule(this);
                    }
                }
            }
        }

        private IDictionary<string, RuleMatch> CreateFragments(
            RuleMatch match,
            SubString text)
        {
            return new Dictionary<string, RuleMatch>() { { _primary.Tag, match } };
        }

        public override string ToString()
        {
            return ToStringRuleName()
                + $"({_primary} - {ToString(_excluded)})";
        }
    }
}