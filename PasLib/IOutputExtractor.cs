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
            IImmutableDictionary<string, RuleMatch> namedChildren);
    }
}