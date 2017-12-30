﻿using System;
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
            IEnumerable<RuleMatch> repeats)
            : this(rule, text, lengthWithInterleaves)
        {
            if (repeats == null)
            {
                throw new ArgumentNullException(nameof(repeats));
            }

            Repeats = ImmutableList<RuleMatch>.Empty.AddRange(repeats);
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IEnumerable<RuleMatch> repeats)
            : this(rule, text, text.Length, repeats)
        {
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IEnumerable<TaggedRuleMatch> fragments)
            : this(rule, text, text.Length, fragments)
        {
        }

        private RuleMatch(
            IRule rule,
            SubString text,
            int lengthWithInterleaves,
            IEnumerable<TaggedRuleMatch> fragments)
            : this(rule, text, lengthWithInterleaves)
        {
            if (fragments == null)
            {
                throw new ArgumentNullException(nameof(fragments));
            }

            Fragments = fragments.Any()
                ? ImmutableList<TaggedRuleMatch>.Empty.AddRange(fragments)
                : null;
        }

        public static RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        public IRule Rule { get; }

        public SubString Text { get; }

        public int LengthWithInterleaves { get; }

        public IImmutableList<RuleMatch> Repeats { get; } = ImmutableList<RuleMatch>.Empty;

        public IImmutableList<TaggedRuleMatch> Fragments { get; }

        public RuleMatch GetFragments(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentNullException(nameof(tag));
            }
            if (Fragments == null)
            {
                throw new IndexOutOfRangeException("There are no fragments");
            }

            foreach (var taggedMatch in Fragments)
            {
                if (taggedMatch.Tag == tag)
                {
                    return taggedMatch.Match;
                }
            }

            throw new IndexOutOfRangeException($"No fragment with tag '{tag}'");
        }

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
                return Fragments == null
                    ? new RuleMatch(rule, Text, LengthWithInterleaves, Repeats)
                    : new RuleMatch(rule, Text, LengthWithInterleaves, Fragments);
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
                return Fragments == null
                    ? new RuleMatch(Rule, Text, LengthWithInterleaves + length, Repeats)
                    : new RuleMatch(Rule, Text, LengthWithInterleaves + length, Fragments);
            }
        }

        public RuleMatch RemoveChildren()
        {
            return Fragments == null && !Repeats.Any()
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