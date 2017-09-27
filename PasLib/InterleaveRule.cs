using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal class InterleaveRule : RuleBase
    {
        private readonly IRule _proxiedRule;

        private InterleaveRule(IRule proxiedRule) : base(proxiedRule.RuleName)
        {
            _proxiedRule = proxiedRule;
        }

        public static IRule FromSequence(SequenceRule rule, IRule interleaveRule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (interleaveRule == null)
            {
                throw new ArgumentNullException(nameof(interleaveRule));
            }

            var repeatInterleaveRule = RepeatInterleave(interleaveRule);

            //  Add non-tagged interleaves on the right of each rule in the sequence
            return new SequenceRule(
                rule.RuleName,
                Alternate(rule.Rules, repeatInterleaveRule));
        }

        public static IRule InterleaveRightOf(IRule rule, IRule interleaveRule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (interleaveRule == null)
            {
                throw new ArgumentNullException(nameof(interleaveRule));
            }

            var repeatInterleaveRule = RepeatInterleave(interleaveRule);

            //  Add non-tagged interleaves on the right of the rule (in a sequence)
            return new InterleaveRule(new SequenceRule(
                rule.RuleName,
                new[]
                {
                    new TaggedRule("$proxied-rule$", rule),
                    new TaggedRule(repeatInterleaveRule)
                }));
        }

        public static IRule InterleaveLeftOf(IRule rule, IRule interleaveRule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (interleaveRule == null)
            {
                throw new ArgumentNullException(nameof(interleaveRule));
            }

            var repeatInterleaveRule = RepeatInterleave(interleaveRule);

            //  Add non-tagged interleaves on the right of the rule (in a sequence)
            return new InterleaveRule(new SequenceRule(
                rule.RuleName,
                new[]
                {
                    new TaggedRule(repeatInterleaveRule),
                    new TaggedRule("$proxied-rule$", rule)
                }));
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var matches = _proxiedRule.Match(context);

            foreach (var m in matches)
            {
                var realMatch = m.Fragments.First().Value;

                yield return realMatch;
            }
        }

        private static IEnumerable<TaggedRule> Alternate(
            IEnumerable<TaggedRule> rules,
            IRule interleaveRule)
        {
            var interleaveTaggedRule = new TaggedRule(interleaveRule);

            foreach (var r in rules)
            {
                yield return r;
                yield return interleaveTaggedRule;
            }
        }

        private static IRule RepeatInterleave(IRule interleaveRule)
        {
            return new RepeatRule(
                "$repeat-interleave$",
                interleaveRule,
                null,
                null);
        }
    }
}