using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GramParserLib.Output
{
    internal class TextOutput : IRuleOutput
    {
        private TextOutput()
        {
        }

        public static IRuleOutput Instance { get => new TextOutput(); }

        object IRuleOutput.ComputeOutput(SubString text, object defaultOutput)
        {
            return text;
        }
    }
}