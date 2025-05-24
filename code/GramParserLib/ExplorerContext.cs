using GramParserLib.Rule;
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

        private const int DEFAULT_MAX_DEPTH = 15;
        private static readonly RuleMatch[] EMPTY_RULE_MATCHES = Array.Empty<RuleMatch>();

        private readonly IRule? _interleaveRule;
        private readonly bool _isInterleaveMatched;
        private readonly IImmutableSet<IRule> _ruleExcepts;
        private readonly AmbiantRuleProperties _ambiantRuleProperties;

        public ExplorerContext(
            SubString text,
            IRule? interleaveRule = null,
            int? depth = null)
            : this(
                  text,
                  interleaveRule,
                  false,
                  depth ?? DEFAULT_MAX_DEPTH,
                  ImmutableHashSet<IRule>.Empty,
                  new AmbiantRuleProperties())
        {
        }

        private ExplorerContext(
            SubString text,
            IRule? interleaveRule,
            bool isInterleaveMatched,
            int depth,
            IImmutableSet<IRule> ruleNameExcepts,
            AmbiantRuleProperties ambiantRuleProperties)
        {
            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }
            _interleaveRule = interleaveRule;
            _isInterleaveMatched = isInterleaveMatched;
            _ruleExcepts = ruleNameExcepts;
            _ambiantRuleProperties = ambiantRuleProperties;
            Text = text;
            Depth = depth;
            ContextID = Guid.NewGuid().ToString();
        }

        public SubString Text { get; }

        public int Depth { get; }

        /// <summary>For debug purposes only.</summary>
        /// <remarks>
        /// Ease the use of conditional breakpoints in the highly recursive rule resolution.
        /// </remarks>
        public string ContextID { get; }

        public ExplorerContext MoveForward(RuleMatch match)
        {
            if (match.LengthWithInterleaves > 0)
            {
                return new ExplorerContext(
                    Text.Skip(match.LengthWithInterleaves),
                    _interleaveRule,
                    false,
                    DEFAULT_MAX_DEPTH,
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
            if (Depth <= 1)
            {
                throw new ParsingException("Recursion exceeds limits");
            }

            var newAmbiantRuleProperties = _ambiantRuleProperties.Merge(rule);
            var newExcepts = GetNewRuleExceptions(newAmbiantRuleProperties, rule);

            if (newExcepts != null)
            {
                var interleaveLength = _isInterleaveMatched ? 0 : MatchInterleave();
                var newText = Text.Skip(interleaveLength);
                var newContext = new ExplorerContext(
                    newText,
                    _interleaveRule,
                    true,
                    Depth - 1,
                    newExcepts,
                    newAmbiantRuleProperties);
                var ruleMatches = rule.Match(newContext);
                var uniqueRuleMatchesWithInterleaves = new UniqueRuleMatchEnumerable(ruleMatches)
                    .Select(m => m.AddInterleaveLength(interleaveLength));

                return uniqueRuleMatchesWithInterleaves;
            }
            else
            {
                return EMPTY_RULE_MATCHES;
            }
        }

        public ExplorerContext SubContext(int length)
        {
            if (length > Text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return new ExplorerContext(
                Text.Take(length),
                _interleaveRule,
                true,
                Depth,
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
            return $"[{Depth}]{Text}";
        }
        #endregion

        private bool UseInterleave()
        {
            return _interleaveRule != null && _ambiantRuleProperties.HasInterleave;
        }

        private ExplorerContext ForInterleave()
        {
            return new ExplorerContext(
                Text,
                null,
                false,
                Depth,
                _ruleExcepts,
                _ambiantRuleProperties);
        }

        private IImmutableSet<IRule>? GetNewRuleExceptions(
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
            if (_interleaveRule == null)
            {
                return 0;
            }
            else
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
}