using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class RepeatRule : RuleBase
    {
        private readonly IRule _rule;
        private readonly int? _min;
        private readonly int? _max;
        private readonly bool _doIncludeChildren;
        private readonly bool _doIncludeGrandChildren;

        public RepeatRule(
            string ruleName,
            IRule interleave,
            IRule rule,
            int? min,
            int? max,
            bool doIncludeChildren,
            bool doIncludeGrandChildren)
            : base(ruleName, interleave)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (min.HasValue && max.HasValue && min.Value > max.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Must be larger than min");
            }
            if (!doIncludeChildren && doIncludeGrandChildren)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(doIncludeGrandChildren),
                    "Can't include grand children if children aren't included");
            }

            _rule = rule;
            _min = min;
            _max = max;
            _doIncludeChildren = doIncludeChildren;
            _doIncludeGrandChildren = doIncludeGrandChildren;
        }

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            var children = new List<RuleMatch>();
            var originalText = text;
            int totalMatchLength = 0;
            int i = 0;

            while (true)
            {
                var result = _rule.Match(text, trace);

                if (result.IsSuccess)
                {
                    if (_doIncludeChildren)
                    {
                        if (_doIncludeGrandChildren)
                        {
                            children.Add(result.Match);
                        }
                        else
                        {
                            var trimmedMatch = new RuleMatch(
                                result.Match.RuleName,
                                result.Match.MatchLength,
                                text.Take(result.Match.MatchLength));

                            children.Add(trimmedMatch);
                        }
                    }
                    ++i;
                    totalMatchLength += result.Match.MatchLength;
                    text = text.Skip(result.Match.MatchLength);
                }
                else
                {
                    if ((!_min.HasValue || i >= _min.Value)
                        && (!_max.HasValue || i <= _max.Value))
                    {
                        return _doIncludeGrandChildren
                            ? new RuleResult(new RuleMatch(RuleName, totalMatchLength, children))
                            : new RuleResult(new RuleMatch(RuleName, totalMatchLength, originalText.Take(totalMatchLength)));
                    }
                    else
                    {
                        return result;
                    }
                }
            }
        }

        public override string ToString()
        {
            var min = _min.HasValue ? _min.Value.ToString() : string.Empty;
            var max = _max.HasValue ? _max.Value.ToString() : string.Empty;

            return "<" + RuleName + "> (" + _rule.ToString() + ")^{" + min + "," + max + "}";
        }
    }
}