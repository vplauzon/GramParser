using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

            var interleaveMatch = UseInterleave()
                ? _interleaveRule.Match(ForInterleave()).FirstOrDefault()
                : null;
            var newAmbiantRuleProperties = _ambiantRuleProperties.Merge(rule);
            var ruleMatches = InvokeRuleIfRecursionValid(
                rule,
                interleaveMatch,
                newAmbiantRuleProperties);

            foreach (var m in ruleMatches)
            {
                if (interleaveMatch != null)
                {
                    yield return m.ChangeText(_text.Take(
                        interleaveMatch.Text.Length
                        + m.Text.Length));
                }
                else
                {
                    yield return m;
                }
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

        public RuleMatch MoveInterleaveRight(RuleMatch match)
        {
            if (UseInterleave())
            {
                var newContext = MoveForward(match).ForInterleave();
                var interleaveMatch = _interleaveRule.Match(newContext).FirstOrDefault();

                return match.ChangeText(
                    _text.Take(match.Text.Length + interleaveMatch.Text.Length));
            }
            else
            {
                return match;
            }
        }

        #region object methods
        public override string ToString()
        {
            return $"[{_depth}]{_text}";
        }
        #endregion

        private bool UseInterleave()
        {
            return _interleaveRule != null && _ambiantRuleProperties.HasInterleave;
        }

        private ExplorerContext ForInterleave()
        {
            return new ExplorerContext(
                _text,
                null,
                _depth,
                _ruleNameExcepts,
                _ambiantRuleProperties);
        }

        private IEnumerable<RuleMatch> InvokeRuleIfRecursionValid(
            IRule rule,
            RuleMatch interleaveMatch,
            AmbiantRuleProperties newAmbiantRuleProperties)
        {
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
                    return InvokeRuleOnly(
                        rule,
                        interleaveMatch,
                        newAmbiantRuleProperties,
                        newExcepts);
                }
            }
            else
            {
                return InvokeRuleOnly(
                    rule,
                    interleaveMatch,
                    newAmbiantRuleProperties,
                    _ruleNameExcepts);
            }
        }

        private IEnumerable<RuleMatch> InvokeRuleOnly(
            IRule rule,
            RuleMatch interleaveMatch,
            AmbiantRuleProperties newAmbiantRuleProperties,
            ImmutableHashSet<IRule> newExcepts)
        {
            var newText = interleaveMatch == null
                ? _text
                : _text.Skip(interleaveMatch.Text.Length);
            var newContext = new ExplorerContext(
                newText,
                _interleaveRule,
                _depth - 1,
                newExcepts,
                newAmbiantRuleProperties);

            return rule.Match(newContext);
        }
    }
}