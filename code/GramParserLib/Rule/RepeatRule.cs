using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib.Rule
{
    internal class RepeatRule : RuleBase
    {
        private readonly IRule _rule;
        private readonly int? _min;
        private readonly int? _max;

        public RepeatRule(
            string ruleName,
            IRuleOutput outputExtractor,
            IRule rule,
            int? min,
            int? max,
            bool? hasInterleave = null,
            bool? isRecursive = null)
            : base(
                  ruleName,
                  outputExtractor,
                  hasInterleave,
                  isRecursive,
                  false)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
            if (min.HasValue && max.HasValue && min.Value > max.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Must be larger than min");
            }

            _min = min;
            _max = max;
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var matches = RecurseMatch(
                context.ContextID,
                context,
                context.Text,
                0,
                1,
                ImmutableList<RuleMatch>.Empty);

            foreach (var m in matches)
            {
                yield return m;
            }
            //  We are returning the matches in decreasing order of text length, so the empty one goes last
            if (!_min.HasValue || _min.Value == 0)
            {
                var matchText = context.Text.Take(0);

                yield return new RuleMatch(
                    this,
                    matchText,
                    () => RuleOutput.ComputeOutput(
                        matchText,
                        new Lazy<object>(ImmutableArray<object>.Empty)));
            }
        }

        public override string ToString()
        {
            var min = _min.HasValue ? _min.Value.ToString() : string.Empty;
            var max = _max.HasValue ? _max.Value.ToString() : string.Empty;

            return ToStringRuleName() + $" ({ToString(_rule)}){{{min}, {max}}}";
        }

        private IEnumerable<RuleMatch> RecurseMatch(
            //  Used only for debugging purposes, to hook on the context ID of the entire sequence
            int masterContextID,
            ExplorerContext context,
            SubString originalText,
            int totalMatchLength,
            int iteration,
            ImmutableList<RuleMatch> childrenMatches)
        {
            var matches = context.InvokeRule(_rule);
            var nonEmptyMatches = matches.Where(m => m.Text.Length != 0);

            foreach (var match in nonEmptyMatches)
            {
                var newTotalMatchLength = totalMatchLength + match.LengthWithInterleaves;
                var newChildrenMatches = childrenMatches.Add(match);

                if (IsRepeatCountBelowMaximum(iteration + 1))
                {   //  Recurse to next iteration
                    var newContext = context.MoveForward(match);
                    var downstreamMatches = RecurseMatch(
                        masterContextID,
                        newContext,
                        originalText,
                        newTotalMatchLength,
                        iteration + 1,
                        newChildrenMatches);

                    foreach (var m in downstreamMatches)
                    {
                        yield return m;
                    }
                }
                //  We are returning the matches in decreasing order of text length, so the "current" one goes last
                if (IsRepeatCountInRange(iteration))
                {
                    var matchText = originalText.Take(newTotalMatchLength);
                    var completeMatch = new RuleMatch(
                        this,
                        matchText,
                        () => ComputeOutput(matchText, newChildrenMatches));

                    yield return completeMatch;
                }
            }
        }

        private object ComputeOutput(
            SubString matchText,
            ImmutableList<RuleMatch> newChildrenMatches)
        {
            Func<object> outputFactory = () => newChildrenMatches
                .Select(m => m.ComputeOutput())
                .ToImmutableArray();

            return RuleOutput.ComputeOutput(matchText, new Lazy<object>(outputFactory));
        }

        private bool IsRepeatCountInRange(int repeatCount)
        {
            return IsRepeatCountBelowMaximum(repeatCount)
                && (!_min.HasValue || _min.Value <= repeatCount);
        }

        private bool IsRepeatCountBelowMaximum(int repeatCount)
        {
            return !_max.HasValue || _max.Value >= repeatCount;
        }
    }
}