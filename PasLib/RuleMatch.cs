using System;
using System.Linq;
using System.Collections.Generic;

namespace PasLib
{
    internal class RuleMatch
    {
        private static readonly RuleMatch[] EMPTY_CONTENTS = new RuleMatch[0];

        public RuleMatch(IRule rule, SubString text, int lengthWithInterleaves)
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

        public RuleMatch(
            IRule rule,
            SubString text,
            int lengthWithInterleaves,
            IEnumerable<RuleMatch> repeats)
            : this(rule, text, lengthWithInterleaves)
        {
            Repeats = repeats ?? throw new ArgumentNullException(nameof(repeats));
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
            int lengthWithInterleaves,
            IEnumerable<KeyValuePair<string, RuleMatch>> fragments)
            : this(rule, text, lengthWithInterleaves)
        {
            if (fragments == null)
            {
                throw new ArgumentNullException(nameof(fragments));
            }

            Fragments = fragments.Any()
                ? new Dictionary<string, RuleMatch>(fragments)
                : null;
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IEnumerable<KeyValuePair<string, RuleMatch>> fragments)
            : this(rule, text, text.Length, fragments)
        {
        }

        public static RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        public IRule Rule { get; }

        public SubString Text { get; }

        public int LengthWithInterleaves { get; }

        public IEnumerable<RuleMatch> Repeats { get; } = EMPTY_CONTENTS;

        public IDictionary<string, RuleMatch> Fragments { get; }

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
    }
}