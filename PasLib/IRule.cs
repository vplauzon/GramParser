using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal interface IRule
    {
        /// <summary>Name of the rule.  Can be <c>null</c> for inline rules.</summary>
        string RuleName { get; }

        /// <summary>Interleave rule.  Can be <c>null</c> if no interleave are defined for this rule.</summary>
        IRule Interleave { get; }

        RuleResult Match(SubString text, RuleTrace trace);
    }
}