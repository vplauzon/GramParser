using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal struct RuleResult
    {
        public RuleResult(RuleMatch match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }
            Match = match;
            Unmatched = new SubString();
        }

        public RuleResult(SubString unmatched)
        {
            if (unmatched.IsNull)
            {
                throw new ArgumentNullException(nameof(unmatched));
            }
            Match = null;
            Unmatched = unmatched;
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

        public RuleMatch Match { get; private set; }
        public SubString Unmatched { get; private set; }

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