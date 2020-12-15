using System.Collections.Immutable;

namespace GramParserLib.Output
{
    internal class ConstantOutput : IRuleOutput
    {
        private readonly object _constant;

        public ConstantOutput(object constant)
        {
            _constant = constant;
        }

        object IRuleOutput.ComputeOutput(SubString text, object defaultOutput)
        {
            return _constant;
        }
    }
}