using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class SubstractRule : RuleBase
    {
        private readonly TaggedRule _primary;
        private readonly IRule _excluded;

        public SubstractRule(string ruleName, IRule interleave, TaggedRule primary, IRule excluded)
            : base(ruleName, interleave)
        {
            if (primary == null)
            {
                throw new ArgumentNullException(nameof(primary));
            }
            if (excluded == null)
            {
                throw new ArgumentNullException(nameof(excluded));
            }

            _primary = primary;
            _excluded = excluded;
        }

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            var primaryResult = _primary.Rule.Match(text, trace);

            if (primaryResult.IsFailure)
            {   //  Failure
                return primaryResult;
            }
            else
            {
                var primaryText = text.Take(primaryResult.Match.MatchLength);
                var excludedResult = _excluded.Match(primaryText, trace);

                if (excludedResult.IsFailure || excludedResult.Match.MatchLength != primaryText.Length)
                {   //  Success
                    var match = primaryResult.Match;

                    return _primary.HasTag
                        ? new RuleResult(new RuleMatch(RuleName, match.MatchLength, CreateFragments(match, text)))
                        : new RuleResult(new RuleMatch(RuleName, match.MatchLength, text.Take(match.MatchLength)));
                }
                else
                {   //  Failure
                    return new RuleResult(text);
                }
            }
        }

        private IDictionary<string, RuleMatch> CreateFragments(RuleMatch match, SubString text)
        {
            if (_primary.DoIncludeGrandChildren)
            {
                return new Dictionary<string, RuleMatch>() { { _primary.Tag, match } };
            }
            else
            {
                var trimmedMatch = new RuleMatch(match.RuleName, match.MatchLength, text.Take(match.MatchLength));

                return new Dictionary<string, RuleMatch>() { { _primary.Tag, trimmedMatch } };
            }
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (" + _primary.ToString() + " - " + _excluded.ToString() + ")";
        }
    }
}