using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Rule
{
    internal class LiteralRule : RuleBase
    {
        #region Inner Types
        private class CaseInsensitiveCharComparer : IEqualityComparer<char>
        {
            public static IEqualityComparer<char> Instance { get; } = new CaseInsensitiveCharComparer();

            private CaseInsensitiveCharComparer()
            {
            }

            bool IEqualityComparer<char>.Equals(char x, char y)
            {
                return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
            }

            int IEqualityComparer<char>.GetHashCode(char obj)
            {
                return char.ToUpperInvariant(obj).GetHashCode();
            }
        }

        private class CaseSensitiveCharComparer : IEqualityComparer<char>
        {
            public static IEqualityComparer<char> Instance { get; } = new CaseSensitiveCharComparer();

            private CaseSensitiveCharComparer()
            {
            }

            bool IEqualityComparer<char>.Equals(char x, char y)
            {
                return x == y;
            }

            int IEqualityComparer<char>.GetHashCode(char obj)
            {
                return obj.GetHashCode();
            }
        }
        #endregion

        private readonly char[] _literal;

        public LiteralRule(
            string? ruleName,
            IRuleOutput? ruleOutput,
            IEnumerable<char> literal,
            bool? isCaseSensitive = null)
            : base(ruleName, ruleOutput, false, isCaseSensitive)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            _literal = literal.ToArray();
        }

        public LiteralRule(
            string? ruleName,
            IRuleOutput? outputExtractor,
            string literal,
            bool? isCaseSensitive = null)
            : base(ruleName, outputExtractor, false, isCaseSensitive)
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
                && text.Take(_literal.Length).SequenceEqual(_literal, GetCharComparer()))
            {
                var matchText = text.Take(_literal.Length);
                var match = new RuleMatch(
                    this,
                    matchText,
                    () => RuleOutput.ComputeOutput(
                        matchText,
                        new Lazy<object?>(() => matchText)));

                return [match];
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

        private IEqualityComparer<char> GetCharComparer()
        {
            return IsCaseSensitive == false
                ? CaseInsensitiveCharComparer.Instance
                : CaseSensitiveCharComparer.Instance;
        }
    }
}