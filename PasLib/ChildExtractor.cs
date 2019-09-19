using System.Collections.Immutable;

namespace PasLib
{
    internal class ChildExtractor : IOutputExtractor
    {
        private readonly string _name;

        public ChildExtractor(string name)
        {
            _name = name;
        }

        object IOutputExtractor.ExtractOutput(
            SubString text,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            RuleMatch child;

            if (namedChildren.TryGetValue(_name, out child))
            {
                return child.Output;
            }
            else
            {
                var childList = string.Join(", ", namedChildren.Keys);

                throw new ParsingException(
                    $"Can't find child '{_name}' for match '{text.ToString()}', only "
                    + $"{{{childList}}}");
            }
        }
    }
}