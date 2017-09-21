using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal abstract class RuleBase : IRule
    {
        protected RuleBase(string ruleName, IRule interleave)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            RuleName = ruleName;
            Interleave = interleave;
        }

        public string RuleName { get; private set; }

        public IRule Interleave { get; private set; }

        public RuleResult Match(SubString text, TracePolicy tracePolicy)
        {
            if (Interleave == null)
            {
                var result = OnMatch(text, tracePolicy);

                return result;
            }
            else
            {
                //  Remove interleaves before
                int before = SeekInterleaves(text, tracePolicy);

                text = text.Skip(before);

                var result = OnMatch(text, tracePolicy);

                if (result.IsFailure)
                {
                    return result;
                }
                else
                {
                    //  Remove interleaves after
                    text = text.Skip(result.Match.MatchLength);

                    var after = SeekInterleaves(text, tracePolicy);

                    return new RuleResult(
                        result.Rule,
                        result.Match.IncreaseMatchLength(before + after),
                        result.Trials);
                }
            }
        }

        protected abstract RuleResult OnMatch(SubString text, TracePolicy tracePolicy);

        protected int SeekInterleaves(SubString text, TracePolicy tracePolicy)
        {
            if (Interleave == null)
            {
                return 0;
            }
            else
            {
                int total = 0;

                while (true)
                {
                    var result = Interleave.Match(text, tracePolicy);

                    if (result.IsSuccess && result.Match.MatchLength != 0)
                    {
                        total += result.Match.MatchLength;
                        text = text.Skip(result.Match.MatchLength);
                    }
                    else
                    {
                        return total;
                    }
                }
            }
        }
    }
}