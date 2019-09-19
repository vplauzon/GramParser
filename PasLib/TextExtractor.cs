using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PasLib
{
    internal class TextExtractor : IOutputExtractor
    {
        object IOutputExtractor.ExtractOutput(
            SubString text,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            return text;
        }
    }
}