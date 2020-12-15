using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GramParserLib.Output
{
    internal class IdentityOutput : IRuleOutput
    {
        object IRuleOutput.ComputeOutput(SubString text, object defaultOutput)
        {
            return default;
        }
    }
}