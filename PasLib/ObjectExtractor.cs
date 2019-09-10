using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
            var outputPairs = (from pair in _extractorPairs
                               let rawKey = pair.Key.ExtractOutput(text, namedChildren)
                               let key = ExtractorHelper.CleanOutput(rawKey) as string
                               let rawValue = pair.Value.ExtractOutput(text, namedChildren)
                               let value = ExtractorHelper.CleanOutput(rawValue)
                               select new
                               {
                                   KeyExtractor = pair.Key,
                                   Key = key,
                                   ValueExtractor = pair.Value,
                                   Value = value
                               }).ToArray();
            var firstNullKey = outputPairs.FirstOrDefault(p => p.Key == null);

            if (firstNullKey != null)
            {
                throw new ParsingException(
                    $"Object's key doesn't resolve to a string:  {firstNullKey.KeyExtractor}");
            }
            else
            {
                var keyValuePairs = from p in outputPairs
                                    select KeyValuePair.Create(p.Key, p.Value);
                var dictionary = ImmutableDictionary<string, object>.Empty.AddRange(keyValuePairs);

                return dictionary;
            }
        }
    }
}