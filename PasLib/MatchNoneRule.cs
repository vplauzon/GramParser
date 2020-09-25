using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PasLib
{
    internal class MatchNoneRule : RuleBase
    {
        private static readonly MatchNoneRule _instance = new MatchNoneRule();

        public static IRule Instance { get { return _instance; } }

        private MatchNoneRule()
            : base(null, null, false, false, true, false)
        {
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            return RuleMatch.EmptyMatch;
        }

        protected override object DefaultExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            throw new NotSupportedException("This should never be fired as this rule never match anything");
        }

        public override string ToString()
        {
            return "(*Match None*)";
        }
    }
}