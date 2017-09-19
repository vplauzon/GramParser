using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal class RuleMatch
    {
        private RuleMatch(string ruleName, int matchLength)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            if (matchLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ruleName));
            }
            RuleName = ruleName;
            MatchLength = matchLength;
        }

        public RuleMatch(string ruleName, int matchLength, SubString content) : this(ruleName, matchLength)
        {
            Content = content;
        }

        public RuleMatch(string ruleName, int matchLength, IEnumerable<RuleMatch> contents) : this(ruleName, matchLength)
        {
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
            Contents = contents;
        }

        public RuleMatch(string ruleName, int matchLength, IDictionary<string, RuleMatch> fragments)
            : this(ruleName, matchLength)
        {
            if (fragments == null || fragments.Count == 0)
            {
                throw new ArgumentNullException(nameof(fragments));
            }
            Fragments = fragments;
        }

        public string RuleName { get; private set; }

        public int MatchLength { get; private set; }

        public SubString Content { get; private set; }

        public IEnumerable<RuleMatch> Contents { get; private set; }

        public IDictionary<string, RuleMatch> Fragments { get; private set; }

        public RuleMatch ChangeName(string ruleName)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }

            if (ruleName == RuleName)
            {
                return this;
            }
            else
            {
                var newMatch = new RuleMatch(ruleName, MatchLength);

                newMatch.Content = Content;
                newMatch.Contents = Contents;
                newMatch.Fragments = Fragments;

                return newMatch;
            }
        }

        public RuleMatch IncreaseMatchLength(int increase)
        {
            if (increase < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(increase));
            }

            if (increase == 0)
            {
                return this;
            }
            else
            {
                var newMatch = new RuleMatch(RuleName, MatchLength);

                newMatch.MatchLength = MatchLength + increase;
                newMatch.Content = Content;
                newMatch.Contents = Contents;
                newMatch.Fragments = Fragments;

                return newMatch;
            }
        }
    }
}