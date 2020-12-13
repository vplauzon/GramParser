using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GramParserLib.Output
{
    internal class TextExtractor : IOutputExtractor
    {
        object IOutputExtractor.ExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            return text;
        }
    }
}