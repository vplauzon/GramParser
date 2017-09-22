using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal interface IRule
    {
        /// <summary>Name of the rule.  Can be <c>null</c> for inline rules.</summary>
        string RuleName { get; }

        RuleResult Match(SubString text, int depth);
    }
}