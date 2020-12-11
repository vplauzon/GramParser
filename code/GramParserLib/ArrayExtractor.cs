using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib
{
    internal class ArrayExtractor : IOutputExtractor
    {
        private readonly IImmutableList<IOutputExtractor> _extractors;

        public ArrayExtractor(IEnumerable<IOutputExtractor> extractors)
        {
            _extractors = ImmutableList<IOutputExtractor>.Empty.AddRange(extractors);
        }

        object IOutputExtractor.ExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            var outputsEnum = from extractor in _extractors
                              let output = extractor.ExtractOutput(text, children, namedChildren)
                              let cleanOutput = ExtractorHelper.StringAsString(output)
                              select cleanOutput;
            var outputs = outputsEnum.ToArray();
            var types = (from o in outputs
                         let type = o != null ? o.GetType() : null
                         group o by type into l
                         select l.Key).ToArray();

            if (types.Length == 1 && types.First() != null)
            {
                var typedOutputs = Array.CreateInstance(types.First(), outputs.Length);

                Array.Copy(outputs, typedOutputs, outputs.Length);

                return typedOutputs;
            }
            else
            {
                return outputs;
            }
        }
    }
}