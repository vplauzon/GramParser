using System;
using System.Linq;
using System.Collections.Generic;

namespace PasLib
{
    internal class RuleMatch
    {
        private static readonly RuleMatch[] EMPTY_CONTENTS = new RuleMatch[0];

        private RuleMatch(IRule rule)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public RuleMatch(IRule rule, SubString text) : this(rule)
        {
            Text = text;
        }

        public RuleMatch(IRule rule, SubString text, IEnumerable<RuleMatch> repeats)
            : this(rule, text)
        {
            Repeats = repeats ?? throw new ArgumentNullException(nameof(repeats));
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IEnumerable<KeyValuePair<string, RuleMatch>> fragments)
            : this(rule, text)
        {
            if (fragments == null)
            {
                throw new ArgumentNullException(nameof(fragments));
            }

            Fragments = fragments.Any()
                ? new Dictionary<string, RuleMatch>(fragments)
                : null;
        }

        public static RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        public IRule Rule { get; private set; }

        public SubString Text { get; private set; }

        public IEnumerable<RuleMatch> Repeats { get; private set; } = EMPTY_CONTENTS;

        public IDictionary<string, RuleMatch> Fragments { get; private set; }

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
                var newMatch = new RuleMatch(rule);

                newMatch.Text = Text;
                newMatch.Repeats = Repeats;
                newMatch.Fragments = Fragments;

                return newMatch;
            }
        }

        public RuleMatch RemoveChildren()
        {
            return Fragments == null && !Repeats.Any()
                ? this
                : new RuleMatch(Rule, Text);
        }

        public RuleMatch ChangeText(SubString text)
        {
            return Fragments == null
                ? new RuleMatch(Rule, text, Repeats)
                : new RuleMatch(Rule, text, Fragments);
        }
    }
}