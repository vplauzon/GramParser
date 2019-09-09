using System.Collections.Immutable;

namespace PasLib
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
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            return _constant;
        }
    }
}