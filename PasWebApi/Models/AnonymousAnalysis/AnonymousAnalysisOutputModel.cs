using PasLib;
using System.Collections.Generic;
using System.Linq;

namespace PasWebApi.Models.AnonymousAnalysis
{
    internal class AnonymousAnalysisOutputModel
    {
        public AnonymousAnalysisOutputModel(RuleMatch match)
        {
            RuleMatch = new RuleMatchModel(match);
        }

        public string ApiVersion { get { return PasWebApi.ApiVersion.FullVersion; } }

        public RuleMatchModel RuleMatch { get; }
    }
}