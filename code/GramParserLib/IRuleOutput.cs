using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GramParserLib
{
    public interface IRuleOutput
    {
        object? ComputeOutput(SubString text, Lazy<object?> lazyDefaultOutput);
    }
}