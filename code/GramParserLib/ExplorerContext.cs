using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib
{
    public class ExplorerContext
    {
        #region Inner Types
        private class UniqueRuleMatchEnumerable : IEnumerable<RuleMatch>
        {
            private readonly IEnumerable<RuleMatch> _source;

            public UniqueRuleMatchEnumerable(IEnumerable<RuleMatch> source)
            {
                _source = source;
            }

            IEnumerator<RuleMatch> IEnumerable<RuleMatch>.GetEnumerator()
            {
                var lengthSet = new HashSet<int>();

                foreach (var match in _source)
                {
                    if (!lengthSet.Contains(match.LengthWithInterleaves))
                    {
                        lengthSet.Add(match.LengthWithInterleaves);

                        yield return match;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<RuleMatch>)this).GetEnumerator();
            }
        }
        #endregion

        private const int DEFAULT_MAX_DEPTH = 40;
        private static readonly RuleMatch[] EMPTY_RULE_MATCHES = new RuleMatch[0];

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
                var uniqueRuleMatchesWithInterleaves = new UniqueRuleMatchEnumerable(ruleMatches)
                    .Select(m => m.AddInterleaveLength(interleaveLength));

#if DEBUG
                //  Useful when debugging and faster than a breakpoint condition
                //if (rule.RuleName == "outputDeclaration")
                //{
                //}

                //  No yield, easier to debug
                var ruleMatchList = uniqueRuleMatchesWithInterleaves.ToArray();

                return ruleMatchList;
#else
                return uniqueRuleMatchesWithInterleaves;
#endif
            }
            else
            {
                return EMPTY_RULE_MATCHES;
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
                var interleaveContext = ForInterleave();

                return MatchInterleave(interleaveContext);
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

        private int MatchInterleave(ExplorerContext interleaveContext)
        {
            var interleaveMatch = _interleaveRule.Match(interleaveContext).FirstOrDefault();

            if (interleaveMatch == null || interleaveMatch.Text.Length == 0)
            {
                return 0;
            }
            else
            {
                var newInterleaveContext = interleaveContext.MoveForward(interleaveMatch);
                //  Recursion
                var remainingLength = MatchInterleave(newInterleaveContext);

                return interleaveMatch.Text.Length + remainingLength;
            }
        }
    }
}