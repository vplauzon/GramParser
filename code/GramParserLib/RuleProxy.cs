using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GramParserLib
{
    internal class RuleProxy : IRule
    {
        private IRule _referencedRule = null;

        public IRule ReferencedRule
        {
            get
            {
                if (_referencedRule == null)
                {
                    throw new InvalidOperationException("Rule isn't set");
                }

                return _referencedRule;
            }
            set
            {
                if (_referencedRule != null)
                {
                    throw new InvalidOperationException("Can't set the referenced rule twice");
                }
                if (value == null)
                {
                    throw new InvalidOperationException("Can't set the referenced rule to null");
                }
                if (!HasNoRecursion(value))
                {
                    throw new ParsingException("Circular rule reference");
                }

                _referencedRule = value;
            }
        }

        #region IRuleProperties
        public bool? HasInterleave { get => ReferencedRule.HasInterleave; }

        public bool? IsRecursive { get => ReferencedRule.IsRecursive; }

        public bool IsTerminalRule { get => ReferencedRule.IsTerminalRule; }

        public bool? HasChildrenDetails { get => ReferencedRule.HasChildrenDetails; }
        #endregion

        string IRule.RuleName => ReferencedRule.RuleName;

        IEnumerable<RuleMatch> IRule.Match(ExplorerContext context)
            => ReferencedRule.Match(context);

        object IRule.ExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            return ReferencedRule.ExtractOutput(text, children, namedChildren);
        }

        private bool HasNoRecursion(IRule value)
        {
            if (ReferenceEquals(this, value))
            {
                return false;
            }
            else
            {
                var proxy = value as RuleProxy;

                return proxy == null
                    || proxy._referencedRule == null
                    || HasNoRecursion(proxy._referencedRule);
            }
        }
    }
}