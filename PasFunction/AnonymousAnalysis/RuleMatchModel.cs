using PasLib;
using System.Collections.Immutable;

namespace PasFunction.AnonymousAnalysis
{
    internal class RuleMatchModel
    {
        public RuleMatchModel(RuleMatch match)
        {
            Rule = match.Rule.RuleName;
            Text = match.Text.ToString();
        }

        public string Rule { get; }

        public string Text { get; }

        public IImmutableList<RuleMatchModel> Repeats { get; }

        public IImmutableDictionary<string, RuleMatch> Fragments { get; }
    }
}