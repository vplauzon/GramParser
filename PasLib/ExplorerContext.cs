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

        public ExplorerContext(SubString text, int? depth = null) : this(
            text,
            depth ?? DEFAULT_MAX_DEPTH,
            ImmutableHashSet<IRule>.Empty)
        {
        }

        private ExplorerContext(
            SubString text,
            int depth,
            ImmutableHashSet<IRule> ruleNameExcepts)
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
                    ImmutableHashSet<IRule>.Empty);
            }
            else
            {
                return this;
            }
        }

        public ExplorerContext TryMoveDown(IRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (!string.IsNullOrWhiteSpace(rule.RuleName))
            {
                if (_depth < 1)
                {
                    throw new InvalidOperationException("Too much recursion");
                }
                else
                {
                    var newSet = _ruleNameExcepts.Add(rule);

                    if (newSet.Count == _ruleNameExcepts.Count)
                    {   //  Recursion into the same rule has been exhausted
                        return null;
                    }
                    else
                    {
                        return new ExplorerContext(_text, _depth - 1, newSet);
                    }
                }
            }
            else
            {
                return this;
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