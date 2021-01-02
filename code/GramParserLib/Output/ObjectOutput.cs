using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib.Output
{
    internal class ObjectOutput : IRuleOutput
    {
        private readonly IImmutableList<KeyValuePair<IRuleOutput, IRuleOutput>> _outputPairs;

        public ObjectOutput(IEnumerable<KeyValuePair<IRuleOutput, IRuleOutput>> outputPairs)
        {
            _outputPairs =
               ImmutableList<KeyValuePair<IRuleOutput, IRuleOutput>>
               .Empty
               .AddRange(outputPairs);
        }

        object IRuleOutput.ComputeOutput(SubString text, Lazy<object> lazyDefaultOutput)
        {
            var outputPairs = (from pair in _outputPairs
                               select new
                               {
                                   KeyExtractor = pair.Key,
                                   Key = pair.Key.ComputeOutput(text, lazyDefaultOutput),
                                   ValueExtractor = pair.Value,
                                   Value = pair.Value.ComputeOutput(text, lazyDefaultOutput)
                               }).ToArray();
            var firstNullKey = outputPairs.FirstOrDefault(p => !(p.Key is string));

            if (firstNullKey != null)
            {
                throw new ParsingException(
                    $"Object's key doesn't resolve to a string:  {firstNullKey.KeyExtractor}");
            }
            else
            {
                var newObjectPairs = from p in outputPairs
                                     select KeyValuePair.Create(p.Key.ToString(), p.Value);
                var dictionary = ImmutableDictionary<string, object>
                    .Empty
                    .AddRange(newObjectPairs);

                return dictionary;
            }
        }
    }
}