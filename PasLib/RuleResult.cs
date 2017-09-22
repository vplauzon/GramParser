using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal struct RuleResult
    {
        private RuleResult(RuleMatch match)
        {
            Rule = null;
            Match = match ?? throw new ArgumentNullException(nameof(match));
            Unmatched = new SubString();
        }

        private RuleResult(IRule rule, SubString unmatched)
        {
            if (unmatched.IsNull)
            {
                throw new ArgumentNullException(nameof(unmatched));
            }
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Match = null;
            Unmatched = unmatched;
        }

        public static RuleResult Success(RuleMatch match)
        {
            return new RuleResult(match);
        }

        public static RuleResult Failure(IRule rule, SubString unmatched)
        {
            return new RuleResult(rule, unmatched);
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

        #region object methods
        public override string ToString()
        {
            return $"{(IsSuccess ? "Success" : "Failure")} rule ${Rule.RuleName}";
        }
        #endregion
    }
}