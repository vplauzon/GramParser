using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal class RuleTrace
    {
        private readonly RuleTrace _stack;
        private readonly IRule _rule;
        private readonly int _length;

        private RuleTrace(RuleTrace stack, IRule rule, int length)
        {
            _stack = stack;
            _rule = rule;
            _length = length;
        }

        public RuleTrace() : this(null, null, 0)
        {
        }

        public RuleTrace Stack(IRule rule, SubString text)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (text.IsNull)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (_stack == null || !IsRecursive(this._stack))
            {
                return new RuleTrace(this, rule, text.Length);
            }
            else
            {
                return null;
            }
        }

        private bool IsRecursive(RuleTrace ruleTrace)
        {
            if (ruleTrace._rule != null)
            {
                if (ruleTrace._length == _length)
                {
                    if (object.ReferenceEquals(_rule, ruleTrace._rule))
                    {
                        return true;
                    }
                    else
                    {
                        return IsRecursive(ruleTrace._stack);
                    }
                }
            }

            return false;
        }
    }
}