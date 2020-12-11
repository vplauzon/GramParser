using System.Collections.Immutable;

namespace GramParserLib
{
    internal class ConstantExtrator : IOutputExtractor
    {
        private readonly object _constant;

        public ConstantExtrator(object constant)
        {
            _constant = constant;
        }

        object IOutputExtractor.ExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            return _constant;
        }
    }
}