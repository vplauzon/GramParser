using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PasLib
{
    internal class ExplorerContext
    {
        private const int DEFAULT_MAX_DEPTH = 40;

        private readonly SubString _text;
        private readonly IRule _interleaveRule;
        private readonly int _depth;
        private readonly ImmutableHashSet<IRule> _ruleNameExcepts;
        private readonly AmbiantRuleProperties _ambiantRuleProperties;

        public ExplorerContext(
            SubString text,
            IRule interleaveRule = null,
            int? depth = null)
            : this(
                  text,
                  interleaveRule,
                  depth ?? DEFAULT_MAX_DEPTH,
                  ImmutableHashSet<IRule>.Empty,
                  new AmbiantRuleProperties())
        {
        }

        private ExplorerContext(
            SubString text,
            IRule interleaveRule,
            int depth,
            ImmutableHashSet<IRule> ruleNameExcepts,
            AmbiantRuleProperties ambiantRuleProperties)
        {
            if (text.IsNull)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }
            _text = text;
            _depth = depth;
            _interleaveRule = interleaveRule;
            _ruleNameExcepts = ruleNameExcepts;
            _ambiantRuleProperties = ambiantRuleProperties
                ?? throw new ArgumentNullException(nameof(ambiantRuleProperties));
        }

        public SubString Text { get => _text; }

        public int Depth { get => _depth; }

        public ExplorerContext MoveForward(RuleMatch match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (match.Text.HasContent)
            {
                return new ExplorerContext(
                    _text.Skip(match.Text.Length),
                    _interleaveRule,
                    _depth,
                    ImmutableHashSet<IRule>.Empty,
                    _ambiantRuleProperties);
            }
            else
            {
                return this;
            }
        }

        public IEnumerable<RuleMatch> InvokeRule(IRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (_depth <= 1)
            {
                throw new InvalidOperationException("Too much recursion");
            }

            var newAmbiantRuleProperties = _ambiantRuleProperties.Merge(rule);

            if (!newAmbiantRuleProperties.IsRecursive
                && !newAmbiantRuleProperties.IsTerminalRule)
            {
                var newExcepts = _ruleNameExcepts.Add(rule);

                if (newExcepts.Count == _ruleNameExcepts.Count)
                {   //  Recursion into the same rule has been exhausted
                    return RuleMatch.EmptyMatch;
                }
                else
                {
                    var newContext = new ExplorerContext(
                        _text,
                        _interleaveRule,
                        _depth - 1,
                        newExcepts,
                        newAmbiantRuleProperties);

                    return rule.Match(newContext);
                }
            }
            else
            {
                var newContext = new ExplorerContext(
                    _text,
                    _interleaveRule,
                    _depth - 1,
                    _ruleNameExcepts,
                    newAmbiantRuleProperties);

                return rule.Match(newContext);
            }
        }

        public ExplorerContext SubContext(int length)
        {
            if (length > _text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return new ExplorerContext(
                _text.Take(length),
                _interleaveRule,
                _depth,
                ImmutableHashSet<IRule>.Empty,
                _ambiantRuleProperties);
        }

        #region object methods
        public override string ToString()
        {
            return $"[{_depth}]{_text}";
        }
        #endregion
    }
}