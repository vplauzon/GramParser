using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PasLib
{
    public class ExplorerContext
    {
        private const int DEFAULT_MAX_DEPTH = 50;

        private readonly SubString _text;
        private readonly IRule _interleaveRule;
        private readonly int _depth;
        private readonly ImmutableHashSet<IRule> _ruleExcepts;
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
            _ruleExcepts = ruleNameExcepts;
            _ambiantRuleProperties = ambiantRuleProperties
                ?? throw new ArgumentNullException(nameof(ambiantRuleProperties));
            ContextID = Guid.NewGuid().GetHashCode();
        }

        public SubString Text { get => _text; }

        public int Depth { get => _depth; }

        /// <summary>For debug purposes only.</summary>
        /// <remarks>
        /// Ease the use of conditional breakpoints in the highly recursive rule resolution.
        /// </remarks>
        public int ContextID { get; }

        public IEnumerable<IRule> RuleExcepts { get { return _ruleExcepts; } }

        public ExplorerContext MoveForward(RuleMatch match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (match.LengthWithInterleaves > 0)
            {
                return new ExplorerContext(
                    _text.Skip(match.LengthWithInterleaves),
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
                throw new ParsingException("Recursion exceeds limits");
            }

            var newAmbiantRuleProperties = _ambiantRuleProperties.Merge(rule);
            var newExcepts = GetNewRuleExceptions(newAmbiantRuleProperties, rule);

            if (newExcepts != null)
            {
                var interleaveLength = MatchInterleave();
                var newText = _text.Skip(interleaveLength);
                var newContext = new ExplorerContext(
                    newText,
                    _interleaveRule,
                    _depth - 1,
                    newExcepts,
                    newAmbiantRuleProperties);
                var ruleMatches = rule.Match(newContext);

                foreach (var m in ruleMatches)
                {
                    yield return m.AddInterleaveLength(interleaveLength);
                }
            }
            else
            {   //  Just for breakpoints, to detect recursion breaks
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

        public int MatchInterleave()
        {
            if (UseInterleave())
            {
                var interleaveMatch =
                    _interleaveRule.Match(ForInterleave()).FirstOrDefault();

                return interleaveMatch == null ? 0 : interleaveMatch.Text.Length;
            }
            else
            {
                return 0;
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
                _ruleExcepts,
                _ambiantRuleProperties);
        }

        private ImmutableHashSet<IRule> GetNewRuleExceptions(
            AmbiantRuleProperties newAmbiantRuleProperties,
            IRule rule)
        {
            if (!string.IsNullOrWhiteSpace(rule.RuleName)
                && !newAmbiantRuleProperties.IsRecursive
                && !newAmbiantRuleProperties.IsTerminalRule)
            {
                if (_ruleExcepts.Contains(rule))
                {   //  Recursion into the same rule has been exhausted
                    return null;
                }
                else
                {
                    return _ruleExcepts.Add(rule);
                }
            }
            else
            {
                return _ruleExcepts;
            }
        }
    }
}