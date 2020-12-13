using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class RangeRule : RuleBase
    {
        public RangeRule(
            string ruleName,
            IOutputExtractor outputExtractor,
            char first,
            char last)
            : base(ruleName, outputExtractor, false, true, true, false)
        {
            if (last < first)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(last),
                    "Must be greater or equal to first");
            }
            First = first;
            Last = last;
        }

        public char First { get; }

        public char Last { get; }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var text = context.Text;

            if (text.HasContent)
            {
                var peek = text.First();

                if (peek >= First && peek <= Last)
                {
                    return new[]
                    {
                        new RuleMatch(this, text.Take(1))
                    };
                }
            }

            return RuleMatch.EmptyMatch;
        }

        protected override object DefaultExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            return text;
        }

        public override string ToString()
        {
            return ToStringRuleName() + $" (\"{First}\"..\"{Last}\")";
        }
    }
}