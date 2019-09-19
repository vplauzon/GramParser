using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PasLib
{
    internal class ThisExtractor : IOutputExtractor
    {
        object IOutputExtractor.ExtractOutput(
            SubString text,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            return text;
        }
    }
}