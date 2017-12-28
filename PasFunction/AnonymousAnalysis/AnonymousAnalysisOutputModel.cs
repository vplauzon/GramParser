using PasLib;
using System.Collections.Generic;
using System.Linq;

namespace PasFunction.AnonymousAnalysis
{
    internal class AnonymousAnalysisOutputModel
    {
        public AnonymousAnalysisOutputModel(RuleMatch match)
        {
            RuleMatch = new RuleMatchModel(match);
        }

        public string ApiVersion { get { return PasFunction.ApiVersion.FullVersion; } }

        public RuleMatchModel RuleMatch { get; }
    }
}