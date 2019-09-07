using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PasLib
{
    public class RuleMatch
    {
        private RuleMatch(IRule rule, SubString text, int lengthWithInterleaves)
        {
            if (lengthWithInterleaves < text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthWithInterleaves));
            }

            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Text = text;
            LengthWithInterleaves = lengthWithInterleaves;
        }

        public RuleMatch(IRule rule, SubString text)
            : this(rule, text, text.Length)
        {
        }

        private RuleMatch(
            IRule rule,
            SubString text,
            int lengthWithInterleaves,
            IEnumerable<RuleMatch> children)
            : this(rule, text, lengthWithInterleaves)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            Children = ImmutableList<RuleMatch>.Empty.AddRange(children);
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IEnumerable<RuleMatch> children)
            : this(rule, text, text.Length, children)
        {
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IImmutableDictionary<string, RuleMatch> namedChildren)
            : this(rule, text, text.Length, namedChildren)
        {
        }

        private RuleMatch(
            IRule rule,
            SubString text,
            int lengthWithInterleaves,
            IImmutableDictionary<string, RuleMatch> namedChildren)
            : this(rule, text, lengthWithInterleaves)
        {
            NamedChildren = namedChildren ?? throw new ArgumentNullException(nameof(namedChildren));
        }

        public static RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        public IRule Rule { get; }

        public SubString Text { get; }

        public object Output { get; }

        public int LengthWithInterleaves { get; }

        public IImmutableList<RuleMatch> Children { get; }
            = ImmutableList<RuleMatch>.Empty;

        public IImmutableDictionary<string, RuleMatch> NamedChildren { get; }
            = ImmutableDictionary<string, RuleMatch>.Empty;

        public RuleMatch ChangeRule(IRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (object.ReferenceEquals(rule, Rule))
            {
                return this;
            }
            else
            {
                return !NamedChildren.Any()
                    ? new RuleMatch(rule, Text, LengthWithInterleaves, Children)
                    : new RuleMatch(rule, Text, LengthWithInterleaves, NamedChildren);
            }
        }

        public RuleMatch AddInterleaveLength(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (length == 0)
            {
                return this;
            }
            else
            {
                return !NamedChildren.Any()
                    ? new RuleMatch(Rule, Text, LengthWithInterleaves + length, Children)
                    : new RuleMatch(Rule, Text, LengthWithInterleaves + length, NamedChildren);
            }
        }

        public RuleMatch RemoveChildren()
        {
            return !NamedChildren.Any() && !Children.Any()
                ? this
                : new RuleMatch(Rule, Text, LengthWithInterleaves);
        }

        #region object methods
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Rule.RuleName)
                ? Text.ToString()
                : $"<{Rule.RuleName}>{Text}";
        }
        #endregion
    }
}