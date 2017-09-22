using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal class RuleMatch
    {
        private RuleMatch(IRule rule)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public RuleMatch(IRule rule, SubString content) : this(rule)
        {
            Content = content;
        }

        public RuleMatch(IRule rule, SubString content, IEnumerable<RuleMatch> contents)
            : this(rule, content)
        {
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        public RuleMatch(IRule rule, SubString content, IDictionary<string, RuleMatch> fragments)
            : this(rule, content)
        {
            if (fragments == null || fragments.Count == 0)
            {
                throw new ArgumentNullException(nameof(fragments));
            }
            Fragments = fragments;
        }

        public IRule Rule { get; private set; }

        public SubString Content { get; private set; }

        public IEnumerable<RuleMatch> Contents { get; private set; }

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

                newMatch.Content = Content;
                newMatch.Contents = Contents;
                newMatch.Fragments = Fragments;

                return newMatch;
            }
        }
    }
}