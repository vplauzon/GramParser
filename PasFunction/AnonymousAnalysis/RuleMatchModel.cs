using PasLib;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PasFunction.AnonymousAnalysis
{
    internal class RuleMatchModel
    {
        public RuleMatchModel(RuleMatch match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

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
                var fragments = from f in match.Fragments
                                select new TaggedRuleMatchModel(f);

                Fragments = fragments.ToArray();
            }
        }

        public string Rule { get; }

        public string Text { get; }

        public RuleMatchModel[] Repeats { get; }

        public TaggedRuleMatchModel[] Fragments { get; }
    }
}