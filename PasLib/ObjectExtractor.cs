using System.Collections.Generic;
using System.Collections.Immutable;

namespace PasLib
{
    internal class ObjectExtractor : IOutputExtractor
    {
        private readonly IImmutableList<KeyValuePair<IOutputExtractor, IOutputExtractor>>
            _extractorPairs;

        public ObjectExtractor(
            IEnumerable<KeyValuePair<IOutputExtractor, IOutputExtractor>> pairs)
        {
            _extractorPairs =
               ImmutableList<KeyValuePair<IOutputExtractor, IOutputExtractor>>
               .Empty
               .AddRange(pairs);
        }

        object IOutputExtractor.ExtractOutput(
            SubString text,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            throw new System.NotImplementedException();
        }
    }
}