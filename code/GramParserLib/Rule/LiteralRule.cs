using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class LiteralRule : RuleBase
    {
        private readonly char[] _literal;

        public LiteralRule(
            string ruleName,
            IRuleOutput ruleOutput,
            IEnumerable<char> literal)
            : base(ruleName, ruleOutput, false, false)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            _literal = literal.ToArray();
        }

        public LiteralRule(
            string ruleName,
            IRuleOutput outputExtractor,
            string literal)
            : base(ruleName, outputExtractor, false, false)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            _literal = literal.ToCharArray();
        }

        public override bool IsTerminalRule => true;

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var text = context.Text;

            if (text.HasContent
                && text.Length >= _literal.Length
                && text.Take(_literal.Length).SequenceEqual(_literal))
            {
                var matchText = text.Take(_literal.Length);
                var match = new RuleMatch(
                    this,
                    matchText,
                    () => RuleOutput.ComputeOutput(
                        matchText,
                        new Lazy<object>(() => matchText)));

                return new[] { match };
            }
            else
            {
                return RuleMatch.EmptyMatch;
            }
        }

        public override string ToString()
        {
            var literal = new string(_literal).Replace("\"", "\\\"");

            return ToStringRuleName() + $" (\"{literal}\")";
        }
    }
}