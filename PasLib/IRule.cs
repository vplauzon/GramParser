using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal interface IRule : IRuleProperties
    {
        /// <summary>Name of the rule.  Can be <c>null</c> for inline rules.</summary>
        string RuleName { get; }

        IEnumerable<RuleMatch> Match(ExplorerContext context);
    }
}