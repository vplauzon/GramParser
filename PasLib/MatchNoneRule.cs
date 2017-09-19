using System;
using System.Collections.Generic;

namespace PasLib
{
    internal class MatchNoneRule : RuleBase
    {
        private static readonly MatchNoneRule _instance = new MatchNoneRule();

        public static IRule Instance { get { return _instance; } }

        private MatchNoneRule() : base(null, null)
        {
        }

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            return new RuleResult(text);
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (*Match None*)";
        }
    }
}