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

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var primaryMatches = _primary.Rule.Match(context);

            foreach (var primaryMatch in primaryMatches)
            {
                var primaryText = primaryMatch.Text;
                var excludingContext = new ExplorerContext(primaryText, context.Depth);
                var excludedMatches = _excluded.Match(excludingContext);
                var excludedExactLength = from ex in excludedMatches
                                          where ex.Text.Length == primaryText.Length
                                          select ex;

                if (!excludedExactLength.Any())
                {
                    if (_primary.HasTag)
                    {
                        var fragment = new KeyValuePair<string, RuleMatch>(
                            _primary.Tag,
                            primaryMatch);

                        yield return new RuleMatch(
                            this,
                            primaryMatch.Text,
                            new[] { fragment });
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
            return $"<{RuleName}> ({_primary.ToString()} - {_excluded.ToString()})";
        }
    }
}