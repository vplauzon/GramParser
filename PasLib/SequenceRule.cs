using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var matches = RecurseMatch(
                Rules,
                context.ContextID,
                context,
                context.Text,
                0,
                ImmutableList<TaggedRuleMatch>.Empty);

            return matches;
        }

        #region object methods
        public override string ToString()
        {
            var rules = from t in Rules
                        select t.ToString();

            return ToStringRuleName()
                + $"({string.Join(" ", rules)})";
        }
        #endregion

        private IEnumerable<RuleMatch> RecurseMatch(
            IEnumerable<TaggedRule> rules,
            //  Used only for debugging purposes, to hook on the context ID of the entire sequence
            int masterContextID,
            ExplorerContext context,
            SubString originalText,
            int totalMatchLength,
            ImmutableList<TaggedRuleMatch> fragments)
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
                        masterContextID,
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