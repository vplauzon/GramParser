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

        protected override RuleResult OnMatch(SubString text, int depth)
        {
            var primaryResult = _primary.Rule.Match(text, depth - 1);

            if (primaryResult.IsSuccess)
            {
                var primaryText = text.Take(primaryResult.Match.Content.Length);
                var excludedResult = _excluded.Match(primaryText, depth - 1);

                if (excludedResult.IsFailure
                    || excludedResult.Match.Content.Length != primaryText.Length)
                {   //  Success
                    var match = primaryResult.Match;

                    return _primary.HasTag
                        ? RuleResult.Success(
                            new RuleMatch(this, match.Content, CreateFragments(match, text)))
                        : RuleResult.Success(
                            new RuleMatch(this, match.Content));
                }
            }

            return RuleResult.Failure(this, text);
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