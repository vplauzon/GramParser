using System;
using System.Collections.Generic;

namespace PasLib
{
    internal class MatchNoneRule : RuleBase
    {
        private static readonly MatchNoneRule _instance = new MatchNoneRule();

        public static IRule Instance { get { return _instance; } }

        private MatchNoneRule() : base(null)
        {
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            return EmptyMatch;
        }

        public override string ToString()
        {
            return "<>(*Match None*)";
        }
    }
}