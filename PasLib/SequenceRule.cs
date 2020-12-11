using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PasLib
{
    internal class SequenceRule : RuleBase
    {
        private readonly TaggedRuleCollection _rules;

        public SequenceRule(
            string ruleName,
            IOutputExtractor outputExtractor,
            IEnumerable<TaggedRule> rules,
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
            if (rules == null || !rules.Any())
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = new TaggedRuleCollection(rules);
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            return RecurseMatch(
                _rules,
                context.ContextID,
                context,
                context.Text,
                0,
                ImmutableList<(TaggedRule, RuleMatch)>.Empty);
        }

        protected override object DefaultExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            if (_rules.HasNames)
            {
                var components = from c in namedChildren
                                 select new { Name = c.Key, Output = c.Value.ComputeOutput() };
                var map = components.ToImmutableDictionary(c => c.Name, c => c.Output);

                return map;
            }
            else
            {
                var subOutputs = from c in children
                                 select c.ComputeOutput();

                return subOutputs.ToArray();
            }
        }

        #region object methods
        public override string ToString()
        {
            var rules = from t in _rules
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
            ImmutableList<(TaggedRule, RuleMatch)> matches)
        {
            var currentRule = rules.First();
            var remainingRules = rules.Skip(1);
            var potentials = context.InvokeRule(currentRule.Rule);

            foreach (var match in potentials)
            {
                var newTotalMatchLength = totalMatchLength + match.LengthWithInterleaves;
                var newMatches = _rules.AddMatch(matches, currentRule, match);

                if (remainingRules.Any())
                {   //  Recurse
                    var newContext = context.MoveForward(match);
                    var downstreamMatches = RecurseMatch(
                        remainingRules,
                        masterContextID,
                        newContext,
                        originalText,
                        newTotalMatchLength,
                        newMatches);

                    foreach (var m in downstreamMatches)
                    {
                        yield return m;
                    }
                }   //  End recursion
                else
                {
                    yield return _rules.CreateMatch(
                        this,
                        originalText.Take(newTotalMatchLength),
                        newMatches);
                }
            }
        }
    }
}