using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class SequenceRule : RuleBase
    {
        private readonly TaggedRule[] _rules;

        public SequenceRule(string ruleName, IRule interleave, IEnumerable<TaggedRule> rules)
            : base(ruleName, interleave)
        {
            if (rules == null || rules.Count() == 0)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _rules = rules.ToArray();
        }

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            var fragments = new Dictionary<string, RuleMatch>();
            var totalMatchLength = 0;
            var originalText = text;

            foreach (var rule in _rules)
            {
                var result = rule.Rule.Match(text, trace);

                if (result.IsSuccess)
                {
                    if (rule.Tag != null)
                    {
                        if (rule.DoIncludeGrandChildren)
                        {
                            fragments[rule.Tag] = result.Match;
                        }
                        else
                        {
                            var trimmedMatch = new RuleMatch(
                                result.Match.RuleName,
                                result.Match.MatchLength,
                                text.Take(result.Match.MatchLength));

                            fragments[rule.Tag] = result.Match;
                        }
                    }
                    text = text.Skip(result.Match.MatchLength);

                    var interleaves = SeekInterleaves(text, trace);

                    text = text.Skip(interleaves);
                    totalMatchLength += result.Match.MatchLength + interleaves;
                }
                else
                {
                    return result;
                }
            }

            return fragments.Any()
                ? new RuleResult(new RuleMatch(RuleName, totalMatchLength, fragments))
                : new RuleResult(
                    new RuleMatch(RuleName, totalMatchLength, originalText.Take(totalMatchLength)));
        }

        public override string ToString()
        {
            var rules = from t in _rules
                        select t.Rule.ToString();

            return "<" + RuleName + "> (" + string.Join(" ", rules)+")";
        }
    }
}