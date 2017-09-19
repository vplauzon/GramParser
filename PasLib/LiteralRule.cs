using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class LiteralRule : RuleBase
    {
        private readonly char[] _literal;

        public LiteralRule(string ruleName, IRule interleave, IEnumerable<char> literal) : base(ruleName, interleave)
        {
            if (literal == null || !literal.Any())
            {
                throw new ArgumentNullException(nameof(literal));
            }

            _literal = literal.ToArray();
        }

        public LiteralRule(string ruleName, IRule interleave, string literal) : base(ruleName, interleave)
        {
            if (string.IsNullOrEmpty(literal))
            {
                throw new ArgumentNullException(nameof(literal));
            }

            _literal = literal.ToCharArray();
        }

        protected override RuleResult OnMatch(SubString text, RuleTrace trace)
        {
            if (text.HasContent
                && text.Length >= _literal.Length
                && text.Enumerate().Take(_literal.Length).SequenceEqual(_literal))
            {
                return new RuleResult(new RuleMatch(RuleName, _literal.Length, text.Take(_literal.Length)));
            }
            else
            {
                return new RuleResult(text);
            }
        }

        public override string ToString()
        {
            return "<" + RuleName + "> (\"" + new string(_literal).Replace("\"", "\\\"") + "\")";
        }
    }
}