using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class SequenceRule : RuleBase
    {
        private static readonly KeyValuePair<string, RuleMatch>[] EMPTY_FRAGMENTS =
            new KeyValuePair<string, RuleMatch>[0];

        private readonly TaggedRule[] _rules;

        public SequenceRule(string ruleName, IEnumerable<TaggedRule> rules)
            : base(ruleName)
        {
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = rules.ToArray();
        }

        protected override IEnumerable<RuleMatch> OnMatch(SubString text, int depth)
        {
            return RecurseMatch(
                _rules,
                text,
                text,
                0,
                depth,
                EMPTY_FRAGMENTS);
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.Rule.ToString();

            return "<" + RuleName + "> (" + string.Join(" ", rules) + ")";
        }

        private IEnumerable<RuleMatch> RecurseMatch(
            IEnumerable<TaggedRule> rules,
            SubString originalText,
            SubString text,
            int totalMatchLength,
            int depth,
            IEnumerable<KeyValuePair<string, RuleMatch>> fragments)
        {
            var currentRule = rules.First();
            var remainingRules = rules.Skip(1);
            var matches = currentRule.Rule.Match(text, depth - 1);

            foreach (var match in matches)
            {
                var newTotalMatchLength = totalMatchLength + match.Content.Length;
                var newFragments = currentRule.Tag == null
                    ? fragments
                    : fragments.Prepend(new KeyValuePair<string, RuleMatch>(
                        currentRule.Tag, match));

                if (remainingRules.Any())
                {   //  Recurse
                    var remainingText = text.Skip(match.Content.Length);
                    var downstreamMatches = RecurseMatch(
                        remainingRules,
                        originalText,
                        remainingText,
                        newTotalMatchLength,
                        depth,
                        newFragments);

                    foreach (var m in downstreamMatches)
                    {
                        yield return m;
                    }
                }   //  End recursion
                else if (newFragments.Any())
                {
                    yield return new RuleMatch(
                        this,
                        originalText.Take(newTotalMatchLength),
                        new Dictionary<string, RuleMatch>(newFragments));
                }
                else
                {
                    yield return new RuleMatch(this, originalText.Take(newTotalMatchLength));
                }
            }
        }
    }
}