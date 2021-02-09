using System;

namespace GramParserWorkbench.Models.Apis
{
    public class DurationModel
    {
        public DurationModel(TimeSpan grammarDuration, TimeSpan matchDuration)
        {
            Grammar = grammarDuration.ToString();
            Match = matchDuration.ToString();
            Total = (grammarDuration + matchDuration).ToString();
        }

        public string Grammar { get; }

        public string Match { get; }

        public string Total { get; }
    }
}