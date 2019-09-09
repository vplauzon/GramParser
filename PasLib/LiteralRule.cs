using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class LiteralRule : RuleBase
    {
        private readonly char[] _literal;

        public LiteralRule(
            string ruleName,
            IOutputExtractor outputExtractor,
            IEnumerable<char> literal)
            : base(ruleName, outputExtractor, false, false, true, false)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            _literal = literal.ToArray();
        }

        public LiteralRule(string ruleName, IOutputExtractor outputExtractor, string literal)
            : base(ruleName, outputExtractor, false, false, true, false)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            _literal = literal.ToCharArray();
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            var text = context.Text;

            if (text.HasContent
                && text.Length >= _literal.Length
                && text.Take(_literal.Length).SequenceEqual(_literal))
            {
                return new[]
                {
                    new RuleMatch(this, text.Take(_literal.Length))
                };
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