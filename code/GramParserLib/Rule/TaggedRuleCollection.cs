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

        public TaggedRuleCollection(IEnumerable<TaggedRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = ImmutableArray<TaggedRule>.Empty.AddRange(rules);

            var withNames = from r in _rules
                            where r.HasTag
                            select r;
            var withoutNames = from r in _rules
                               where !r.HasTag
                               select r;
            var hasWithNames = withNames.Any();
            var hasWithoutNames = withoutNames.Any();

            if (hasWithNames && hasWithoutNames)
            {
                throw new ParsingException(
                    "Can't have both named & unnamed rule match in one rule");
            }

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
    }
}