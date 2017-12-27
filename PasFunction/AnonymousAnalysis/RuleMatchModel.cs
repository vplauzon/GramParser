using PasLib;
using System.Collections.Generic;
using System.Linq;

namespace PasFunction.AnonymousAnalysis
{
    internal class RuleMatchModel
    {
        public RuleMatchModel(RuleMatch match)
        {
            Rule = match.Rule.RuleName;
            Text = match.Text.ToString();
            if (match.Repeats != null && match.Repeats.Count > 0)
            {
                var repeats = from r in match.Repeats
                              select new RuleMatchModel(r);

                Repeats = repeats.ToArray();
            }
            if (match.Fragments != null && match.Fragments.Count > 0)
            {
                var dictionary = new Dictionary<string, RuleMatchModel>();
                
                foreach(var f in match.Fragments)
                {
                    var model = new RuleMatchModel(f.Value);

                    dictionary.Add(f.Key, model);
                }
                Fragments = dictionary;
            }
        }

        public string Rule { get; }

        public string Text { get; }

        public RuleMatchModel[] Repeats { get; }

        public Dictionary<string, RuleMatchModel> Fragments { get; }
    }
}