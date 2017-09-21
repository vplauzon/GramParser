using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal struct RuleResult
    {
        internal RuleResult(IRule rule, RuleMatch match, RuleResult[] trials)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Match = match ?? throw new ArgumentNullException(nameof(match));
            Unmatched = new SubString();
            Trials = trials ?? throw new ArgumentNullException(nameof(trials));
        }

        internal RuleResult(IRule rule, SubString unmatched, RuleResult[] trials)
        {
            if (unmatched.IsNull)
            {
                throw new ArgumentNullException(nameof(unmatched));
            }
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Match = null;
            Unmatched = unmatched;
            Trials = trials ?? throw new ArgumentNullException(nameof(trials));
        }

        public bool IsSuccess
        {
            get
            {
                if (Match != null)
                {
                    return true;
                }
                else if (Unmatched.IsNull)
                {
                    throw new NotSupportedException("RuleResult hasn't been initialized");
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsFailure
        {
            get
            {
                if (!Unmatched.IsNull)
                {
                    return true;
                }
                else if (Match == null)
                {
                    throw new NotSupportedException("RuleResult hasn't been initialized");
                }
                else
                {
                    return false;
                }
            }
        }

        public IRule Rule { get; private set; }

        public RuleMatch Match { get; private set; }

        public SubString Unmatched { get; private set; }

        public RuleResult[] Trials { get; private set; }

        #region object methods
        public override string ToString()
        {
            return IsSuccess
                ? "Success"
                : "Failure";
        }
        #endregion
    }
}