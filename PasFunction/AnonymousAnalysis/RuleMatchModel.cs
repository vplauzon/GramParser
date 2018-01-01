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
            if (match.Children != null && match.Children.Count > 0)
            {
                var children = from r in match.Children
                               select new RuleMatchModel(r);

                Children = children.ToArray();
            }
            if (match.NamedChildren != null && match.NamedChildren.Count > 0)
            {
                var namedChildren = from f in match.NamedChildren
                                    select new NamedRuleMatchModel(f);

                NamedChildren = namedChildren.ToArray();
            }
        }

        public string Rule { get; }

        public string Text { get; }

        public RuleMatchModel[] Children { get; }

        public NamedRuleMatchModel[] NamedChildren { get; }
    }
}