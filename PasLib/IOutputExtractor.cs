using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PasLib
{
    public interface IOutputExtractor
    {
        object ExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren);
    }
}