using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GramParserLib.Output
{
    internal class DefaultOutput : IRuleOutput
    {
        private DefaultOutput()
        {
        }

        public static IRuleOutput Instance { get; } = new DefaultOutput();

        object? IRuleOutput.ComputeOutput(SubString text, Lazy<object?> lazyDefaultOutput)
        {
            return lazyDefaultOutput.Value;
        }
    }
}