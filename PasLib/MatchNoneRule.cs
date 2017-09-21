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

        protected override RuleResult OnMatch(SubString text, TracePolicy tracePolicy)
        {
            return new RuleResult(this, text, tracePolicy.EmptyTrials);
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (*Match None*)";
        }
    }
}