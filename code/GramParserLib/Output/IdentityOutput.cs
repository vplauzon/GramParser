using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GramParserLib.Output
{
    internal class IdentityOutput : IRuleOutput
    {
        private IdentityOutput()
        {
        }

        public static IRuleOutput Instance { get; } = new IdentityOutput();

        object? IRuleOutput.ComputeOutput(SubString text, Lazy<object?> lazyDefaultOutput)
        {
            return lazyDefaultOutput.Value;
        }
    }
}