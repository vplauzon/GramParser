using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class SequenceRule : RuleBase
    {
        public SequenceRule(string ruleName, IEnumerable<TaggedRule> rules)
            : base(ruleName)
        {
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            Rules = rules.ToArray();
        }

        public TaggedRule[] Rules { get; }

        protected override IEnumerable<RuleMatch> OnMatch(SubString text, int depth)
        {
            return RecurseMatch(
                Rules,
                text,
                text,
                0,
                depth,
                TaggedRule.EMPTY_FRAGMENTS);
        }

        public override string ToString()
        {
            var rules = from t in Rules
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
                var newTotalMatchLength = totalMatchLength + match.Text.Length;
                var newFragments = currentRule.AddFragment(fragments, match);

                if (remainingRules.Any())
                {   //  Recurse
                    var remainingText = text.Skip(match.Text.Length);
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
                        newFragments);
                }
                else
                {
                    yield return new RuleMatch(this, originalText.Take(newTotalMatchLength));
                }
            }
        }
    }
}