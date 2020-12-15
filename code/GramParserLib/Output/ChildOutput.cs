using System.Collections.Immutable;

namespace GramParserLib.Output
{
    internal class ChildOutput : IRuleOutput
    {
        private readonly string _name;

        public ChildOutput(string name)
        {
            _name = name;
        }

        object IRuleOutput.ComputeOutput(SubString text, object defaultOutput)
        {
            var map = defaultOutput as IImmutableDictionary<string, object>;
            object child;

            if (map == null)
            {
                throw new ParsingException(
                    $"Rule has no children, so can't find child '{_name}'");
            }
            else if (map.TryGetValue(_name, out child))
            {
                return child;
            }
            else
            {
                var childList = string.Join(", ", map.Keys);

                throw new ParsingException(
                    $"Can't find child '{_name}' for match '{text.ToString()}', only "
                    + $"{{{childList}}}");
            }
        }
    }
}