﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GramParserLib.Rule
{
    /// <summary>
    /// Proxy to a rule that might not have been instantiated yet and can be set later.
    /// This is used to resolve circular dependency.
    /// </summary>
    internal class RuleProxy : IRule
    {
        private IRule? _referencedRule = null;

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
        public bool? HasInterleave => ReferencedRule.HasInterleave;

        public bool IsTerminalRule => ReferencedRule.IsTerminalRule;
        
        public bool? IsCaseSensitive => ReferencedRule.IsCaseSensitive;
        #endregion

        string? IRule.RuleName => ReferencedRule.RuleName;

        IEnumerable<RuleMatch> IRule.Match(ExplorerContext context)
            => ReferencedRule.Match(context);

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