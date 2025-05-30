﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace GramParserLib
{
    public class ExplorerContext
    {
        #region Inner Types
        private class TraceableEnumerable : IEnumerable<RuleMatch>
        {
            private readonly IEnumerable<RuleMatch> _source;
            private readonly Action<bool> _traceAction;

            public TraceableEnumerable(
                IEnumerable<RuleMatch> source,
                Action<bool> traceAction)
            {
                _source = source;
                _traceAction = traceAction;
            }

            IEnumerator<RuleMatch> IEnumerable<RuleMatch>.GetEnumerator()
            {
                bool hasTraced = false;

                foreach (var match in _source)
                {
                    if (!hasTraced)
                    {
                        hasTraced = true;
                        _traceAction(true);
                    }
                    yield return match;
                }
                if (!hasTraced)
                {
                    _traceAction(false);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<RuleMatch>)this).GetEnumerator();
            }
        }
        #endregion

        private const int DEFAULT_MAX_DEPTH = 100;

#if DEBUG
        private static volatile int _contextIdGenerator = 0;
#endif

        private readonly IRule? _interleaveRule;
        private readonly bool _isInterleaveMatched;
        private readonly AmbiantRuleProperties _ambiantRuleProperties;
        private readonly bool _isTracing;

        #region Constructors
        public ExplorerContext(
            SubString text,
            IRule? interleaveRule = null,
            int? maxDepth = null,
            bool isTracing = false)
            : this(
                  text,
                  interleaveRule,
                  false,
                  maxDepth ?? DEFAULT_MAX_DEPTH,
                  isTracing,
#if DEBUG
                  ImmutableArray<int>.Empty,
#endif
                  new AmbiantRuleProperties())
        {
        }

        private ExplorerContext(
            SubString text,
            IRule? interleaveRule,
            bool isInterleaveMatched,
            int depth,
            bool isTracing,
#if DEBUG
            IImmutableList<int> parentContextIDs,
#endif
            AmbiantRuleProperties ambiantRuleProperties)
        {
            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }
            _interleaveRule = interleaveRule;
            _isInterleaveMatched = isInterleaveMatched;
            _isTracing = isTracing;
            _ambiantRuleProperties = ambiantRuleProperties;
            Text = text;
            Depth = depth;
#if DEBUG
            ContextID = Interlocked.Increment(ref _contextIdGenerator);
            ParentContextIDs = parentContextIDs;
#endif
        }
        #endregion

        public SubString Text { get; }

        public int Depth { get; }

#if DEBUG
        /// <summary>For debug purposes only.</summary>
        /// <remarks>
        /// Ease the use of conditional breakpoints in the highly recursive rule resolution.
        /// </remarks>
        public int ContextID { get; }

        /// <summary>For debug purposes only.</summary>
        /// <remarks>
        /// Resolve the parent of <see cref="ContextID"/>.
        /// </remarks>
        public IImmutableList<int> ParentContextIDs { get; }
#endif

        public ExplorerContext MoveForward(RuleMatch match)
        {
            if (match.LengthWithInterleaves > 0)
            {
                return new ExplorerContext(
                    Text.Skip(match.LengthWithInterleaves),
                    _interleaveRule,
                    false,
                    Depth,
                    _isTracing,
#if DEBUG
                    ParentContextIDs.Add(ContextID),
#endif
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
            var interleaveLength = _isInterleaveMatched ? 0 : MatchInterleave();
            var newText = Text.Skip(interleaveLength);
            var newContext = new ExplorerContext(
                newText,
                _interleaveRule,
                true,
                Depth - 1,
                _isTracing,
#if DEBUG
                ParentContextIDs.Add(ContextID),
#endif
                newAmbiantRuleProperties);
            var ruleMatches = rule.Match(newContext);
#if DEBUG
            var doTracing = _isTracing && !string.IsNullOrWhiteSpace(rule.RuleName);
            var indent = new string(' ', Depth);
            var matches = !doTracing
                ? ruleMatches
                : new TraceableEnumerable(
                    ruleMatches,
                    hasMatch =>
                    {
                        Trace.WriteLine(
                            $"{ContextID}|{indent}'{rule.RuleName}' ({Depth}):  ({hasMatch})");
                    });

            if (doTracing)
            {
                Trace.WriteLine(
                    $"{ContextID}|{indent}'{rule.RuleName}' ({Depth}):  '{Text}'");
            }
#else
            var matches = ruleMatches;
#endif
            var matchesWithInterleave = matches
                .Select(m => m.AddInterleaveLength(interleaveLength));

            return matchesWithInterleave;
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
                _isTracing,
#if DEBUG
                ParentContextIDs.Add(ContextID),
#endif
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
                _isTracing,
#if DEBUG
                ParentContextIDs.Add(ContextID),
#endif
                _ambiantRuleProperties);
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