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
            IRule rule,
            int? min,
            int? max,
            bool doIncludeChildren,
            bool doIncludeGrandChildren)
            : base(ruleName)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
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

            _min = min;
            _max = max;
            _doIncludeChildren = doIncludeChildren;
            _doIncludeGrandChildren = doIncludeGrandChildren;
        }

        protected override RuleResult OnMatch(SubString text, int depth)
        {
            var children = new List<RuleMatch>();
            var originalText = text;
            int totalMatchLength = 0;
            int i = 0;

            while (true)
            {
                var result = _rule.Match(text, depth - 1);

                if (result.IsSuccess && result.Match.Content.Length > 0)
                {
                    if (_doIncludeChildren)
                    {
                        children.Add(result.Match);
                    }
                    ++i;
                    totalMatchLength += result.Match.Content.Length;
                    text = text.Skip(result.Match.Content.Length);
                }
                else if ((!_min.HasValue || i >= _min.Value) && (!_max.HasValue || i <= _max.Value))
                {
                    var content = originalText.Take(totalMatchLength);

                    return _doIncludeGrandChildren
                        ? RuleResult.Success(new RuleMatch(this, content, children))
                        : RuleResult.Success(new RuleMatch(this, content));
                }
                else
                {
                    return RuleResult.Failure(this, text);
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