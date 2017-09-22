using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class SubstractRule : RuleBase
    {
        private readonly TaggedRule _primary;
        private readonly IRule _excluded;

        public SubstractRule(string ruleName, TaggedRule primary, IRule excluded)
            : base(ruleName)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _excluded = excluded ?? throw new ArgumentNullException(nameof(excluded));
        }

        protected override IEnumerable<RuleMatch> OnMatch(SubString text, int depth)
        {
            var primaryMatches = _primary.Rule.Match(text, depth - 1);

            foreach (var primaryMatch in primaryMatches)
            {
                var primaryText = primaryMatch.Content;
                var excludedMatches = _excluded.Match(primaryText, depth - 1);
                var excludedExactLength = from ex in excludedMatches
                                          where ex.Content.Length == primaryText.Length
                                          select ex;

                if (!excludedExactLength.Any())
                {
                    if (_primary.HasTag)
                    {
                        var fragments = new Dictionary<string, RuleMatch>() { { _primary.Tag, primaryMatch } };

                        yield return new RuleMatch(
                            this,
                            primaryMatch.Content,
                            fragments);
                    }
                    else
                    {
                        yield return primaryMatch.ChangeRule(this);
                    }
                }
            }
        }

        private IDictionary<string, RuleMatch> CreateFragments(RuleMatch match, SubString text)
        {
            return new Dictionary<string, RuleMatch>() { { _primary.Tag, match } };
        }

        public override string ToString()
        {
            return $"<{RuleName}> ({_primary.ToString()} - {_excluded.ToString()})";
        }
    }
}