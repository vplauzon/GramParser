using GramParserLib;
using System.Collections.Generic;
using System.Linq;

namespace GramParserWorkbench.Models.Apis
{
    internal class SingleOutputModel
    {
        public SingleOutputModel(RuleMatch match)
        {
            IsMatch = match != null;
            if (match != null)
            {
                RuleMatch = new RuleMatchModel(match);
                Output = match.ComputeOutput();
            }
        }

        public string ApiVersion { get { return AppVersion.FullVersion; } }

        public bool IsMatch { get; set; }

        public RuleMatchModel RuleMatch { get; }

        public object Output { get; }
    }
}