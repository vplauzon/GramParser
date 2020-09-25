using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PasLib
{
    public interface IRule : IRuleProperties
    {
        /// <summary>Name of the rule.  Can be <c>null</c> for inline rules.</summary>
        string RuleName { get; }

        IEnumerable<RuleMatch> Match(ExplorerContext context);

        object ExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren);
    }
}