using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PasLib
{
    internal class MatchAnyCharacterRule : RuleBase
    {
        public MatchAnyCharacterRule(string ruleName, IOutputExtractor outputExtractor)
            : base(ruleName, outputExtractor, false, false, true, false)
        {
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var text = context.Text;

            if (text.Length == 0)
            {
                return RuleMatch.EmptyMatch;
            }
            else
            {
                return new[]
                {
                    new RuleMatch(this, text.Take(1))
                };
            }
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
            return ToStringRuleName() + " .";
        }
    }
}
