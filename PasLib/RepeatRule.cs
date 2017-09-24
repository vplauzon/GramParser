using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class RepeatRule : RuleBase
    {
        private static readonly RuleMatch[] EMPTY_CHILDREN = new RuleMatch[0];

        private readonly IRule _rule;
        private readonly int? _min;
        private readonly int? _max;

        public RepeatRule(
            string ruleName,
            IRule rule,
            int? min,
            int? max)
            : base(ruleName)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
            if (min.HasValue && max.HasValue && min.Value > max.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Must be larger than min");
            }

            _min = min;
            _max = max;
        }

        protected override IEnumerable<RuleMatch> OnMatch(SubString text, int depth)
        {
            return RecurseMatch(
                text,
                text,
                0,
                0,
                depth,
                EMPTY_CHILDREN);
        }

        public override string ToString()
        {
            var min = _min.HasValue ? _min.Value.ToString() : string.Empty;
            var max = _max.HasValue ? _max.Value.ToString() : string.Empty;

            return "<" + RuleName + "> (" + _rule.ToString() + ")^{" + min + "," + max + "}";
        }

        private IEnumerable<RuleMatch> RecurseMatch(
            SubString text,
            SubString originalText,
            int totalMatchLength,
            int iteration,
            int depth,
            IEnumerable<RuleMatch> reverseChilden)
        {
            var matches = _rule.Match(text, depth - 1);
            var nonEmptyMatches = from m in matches
                                  where m.Text.Length > 0
                                  select m;

            if (nonEmptyMatches.Any())
            {
                foreach (var match in nonEmptyMatches)
                {
                    var newReverseChildren = reverseChilden.Prepend(match);
                    var newTotalMatchLength = totalMatchLength + match.Text.Length;

                    if (!_max.HasValue || iteration + 1 < _max.Value)
                    {   //  We can still repeat
                        var remainingText = text.Skip(match.Text.Length);
                        var downstreamMatches = RecurseMatch(
                            remainingText,
                            originalText,
                            newTotalMatchLength,
                            iteration + 1,
                            depth,
                            newReverseChildren);

                        foreach (var m in downstreamMatches)
                        {
                            yield return m;
                        }
                    }
                    //  We have reached our max:  end recursion
                    //  Have we reached our min?
                    else if ((!_min.HasValue || iteration + 1 >= _min.Value))
                    {
                        var content = originalText.Take(newTotalMatchLength);

                        yield return new RuleMatch(
                            this,
                            content,
                            newReverseChildren.Reverse().ToArray());
                    }
                }
            }
            //  Repeat didn't work, but if we already reached our min, we're good
            //  (even if no content)
            else if ((!_min.HasValue || iteration >= _min.Value))
            {
                var content = originalText.Take(totalMatchLength);

                yield return new RuleMatch(
                    this,
                    content,
                    reverseChilden.Reverse().ToArray());
            }
        }
    }
}