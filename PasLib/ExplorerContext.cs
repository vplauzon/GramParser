using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PasLib
{
    internal class ExplorerContext
    {
        private const int DEFAULT_MAX_DEPTH = 40;

        private readonly SubString _text;
        private readonly int _depth;
        private readonly ImmutableHashSet<IRule> _ruleNameExcepts;
        private readonly AmbiantRuleProperties _ambiantRuleProperties;

        public ExplorerContext(SubString text, int? depth = null) : this(
            text,
            depth ?? DEFAULT_MAX_DEPTH,
            ImmutableHashSet<IRule>.Empty,
            new AmbiantRuleProperties())
        {
        }

        private ExplorerContext(
            SubString text,
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
                    _depth - 1,
                    _ruleNameExcepts,
                    newAmbiantRuleProperties);

                return rule.Match(newContext);
            }
        }

        #region object methods
        public override string ToString()
        {
            return $"[{_depth}]{_text}";
        }
        #endregion
    }
}