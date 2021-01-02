using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GramParserLib.Output
{
    internal class ArrayOutput : IRuleOutput
    {
        private readonly IImmutableList<IRuleOutput> _outputs;

        public ArrayOutput(IEnumerable<IRuleOutput> extractors)
        {
            _outputs = ImmutableList<IRuleOutput>.Empty.AddRange(extractors);
        }

        object IRuleOutput.ComputeOutput(SubString text, Lazy<object> lazyDefaultOutput)
        {
            var outputs = _outputs
                .Select(o => o.ComputeOutput(text, lazyDefaultOutput))
                .ToImmutableArray();

            return outputs;
        }
    }
}