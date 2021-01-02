using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GramParserLib.Rule
{
    internal class MatchAnyCharacterRule : RuleBase
    {
        public MatchAnyCharacterRule(string? ruleName, IRuleOutput? outputExtractor)
            : base(ruleName, outputExtractor, false, false)
        {
        }

        public override bool IsTerminalRule => true;

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var text = context.Text;

            if (text.Length == 0)
            {
                return RuleMatch.EmptyMatch;
            }
            else
            {
                var matchText = text.Take(1);
                var match = new RuleMatch(
                    this,
                    matchText,
                    () => RuleOutput.ComputeOutput(matchText, new Lazy<object?>(matchText)));

                return new[] { match };
            }
        }

        public override string ToString()
        {
            return ToStringRuleName() + " .";
        }
    }
}
