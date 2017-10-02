using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class SequenceRule : RuleBase
    {
        public SequenceRule(
            string ruleName,
            IEnumerable<TaggedRule> rules,
            bool? hasInterleave = null,
            bool? isRecursive = null)
            : base(ruleName, hasInterleave, isRecursive, false)
        {
            if (rules == null || !rules.Any())
            {
                throw new ArgumentNullException(nameof(rules));
            }

            Rules = rules.ToArray();
        }

        public TaggedRule[] Rules { get; }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            return RecurseMatch(
                Rules,
                context,
                context.Text,
                0,
                TaggedRule.EMPTY_FRAGMENTS);
        }

        public override string ToString()
        {
            var rules = from t in Rules
                        select t.ToString();

            return ToStringRuleName()
                + $"({string.Join(" ", rules)})";
        }

        private IEnumerable<RuleMatch> RecurseMatch(
            IEnumerable<TaggedRule> rules,
            ExplorerContext context,
            SubString originalText,
            int totalMatchLength,
            IEnumerable<KeyValuePair<string, RuleMatch>> fragments)
        {
            var currentRule = rules.First();
            var remainingRules = rules.Skip(1);
            var matches = context.InvokeRule(currentRule.Rule);

            foreach (var match in matches)
            {
                var newTotalMatchLength = totalMatchLength + match.LengthWithInterleaves;
                var newFragments = currentRule.AddFragment(fragments, match);

                if (remainingRules.Any())
                {   //  Recurse
                    var newContext = context.MoveForward(match);
                    var downstreamMatches = RecurseMatch(
                        remainingRules,
                        newContext,
                        originalText,
                        newTotalMatchLength,
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
                    yield return
                        new RuleMatch(this, originalText.Take(newTotalMatchLength));
                }
            }
        }
    }
}