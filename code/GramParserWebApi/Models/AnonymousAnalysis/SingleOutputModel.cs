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
                RuleMatch = new RuleMatchModel(match);
                Output = match.ComputeOutput();
            }
        }

        public string ApiVersion { get { return PasWebApi.ApiVersion.FullVersion; } }

        public bool IsMatch { get; set; }

        public RuleMatchModel RuleMatch { get; }

        public object Output { get; }
    }
}