using PasLib;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.Immutable;

namespace PasWebApi.Models.AnonymousAnalysis
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
                                    select new KeyValuePair<string, RuleMatchModel>(
                                        f.Key,
                                        new RuleMatchModel(f.Value));

                NamedChildren = ImmutableDictionary<string, RuleMatchModel>.Empty.AddRange(
                    namedChildren);
            }
        }

        public string Rule { get; }

        public string Text { get; }

        public RuleMatchModel[] Children { get; }

        public IImmutableDictionary<string, RuleMatchModel> NamedChildren { get; }
    }
}