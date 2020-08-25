using PasLib;
using System.Collections.Generic;
using System.Linq;

namespace PasWebApi.Models.AnonymousAnalysis
{
    internal class SingleOutputModel
    {
        public SingleOutputModel(RuleMatch match)
        {
            IsMatch = match != null;
            if (match != null)
            {
                if (match.Rule.OutputExtractor == null)
                {
                    RuleMatch = new RuleMatchModel(match);
                }
                else
                {
                    Output = match.Output;
                }
            }
        }

        public string ApiVersion { get { return PasWebApi.ApiVersion.FullVersion; } }

        public bool IsMatch { get; set; }

        public RuleMatchModel RuleMatch { get; }

        public object Output { get; }
    }
}