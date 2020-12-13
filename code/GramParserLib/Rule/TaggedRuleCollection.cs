using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class TaggedRuleCollection : IEnumerable<TaggedRule>
    {
        private readonly ImmutableArray<TaggedRule> _rules;
        private readonly bool _hasSelection;

        public TaggedRuleCollection(IEnumerable<TaggedRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = ImmutableArray<TaggedRule>.Empty.AddRange(rules);

            var withNames = from r in _rules
                            where r.HasTag
                            && r.MatchSelectionState != MatchSelection.Unspecified
                            select r;
            var withoutNames = from r in _rules
                               where !r.HasTag
                               && r.MatchSelectionState != MatchSelection.Unspecified
                               select r;
            var hasWithNames = withNames.Any();
            var hasWithoutNames = withoutNames.Any();

            if (hasWithNames && hasWithoutNames)
            {
                throw new ParsingException(
                    "Can't have both named & unnamed rule match in one rule");
            }

            _hasSelection = hasWithNames || hasWithoutNames;
            HasNames = hasWithNames;
        }

        public bool HasNames { get; }

        #region IEnumerable<TaggedRule>
        public IEnumerator<TaggedRule> GetEnumerator()
        {
            return ((IEnumerable<TaggedRule>)_rules).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TaggedRule>)_rules).GetEnumerator();
        }
        #endregion

        public ImmutableList<(TaggedRule, RuleMatch)> AddMatch(
            ImmutableList<(TaggedRule, RuleMatch)> matches,
            TaggedRule rule,
            RuleMatch newMatch)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (newMatch == null)
            {
                throw new ArgumentNullException(nameof(newMatch));
            }

            return !_hasSelection || rule.MatchSelectionState != MatchSelection.Unspecified
                ? matches.Add((rule, newMatch))
                : matches;
        }

        public RuleMatch CreateMatch(
            IRule rule,
            SubString text,
            ImmutableList<(TaggedRule rule, RuleMatch match)> subMatches)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (subMatches == null)
            {
                throw new ArgumentNullException(nameof(subMatches));
            }
            if (subMatches.Any())
            {
                if (HasNames)
                {
                    var namedMatches = from m in subMatches
                                       select new KeyValuePair<string, RuleMatch>(
                                           m.rule.Tag,
                                           FormatMatch(m.match, m.rule));
                    var children =
                        ImmutableDictionary<string, RuleMatch>.Empty.AddRange(namedMatches);

                    return new RuleMatch(rule, text, children);
                }
                else
                {
                    var unnamedMatches = from m in subMatches
                                         select FormatMatch(m.match, m.rule);

                    return new RuleMatch(rule, text, unnamedMatches);
                }
            }
            else
            {
                return new RuleMatch(rule, text);
            }
        }

        private RuleMatch FormatMatch(RuleMatch match, TaggedRule rule)
        {
            return rule.MatchSelectionState == MatchSelection.ChildrenOnly
                ? match.RemoveChildren()
                : match;
        }
    }
}