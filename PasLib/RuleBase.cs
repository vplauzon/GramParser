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

        public RuleResult Match(SubString text, RuleTrace trace)
        {
#if DEBUG
            Func<string, string> escape = s => s.Replace("\n", "\\n").Replace("\r", "\\r");
            var textExtract = escape(text.Take(Math.Min(20, text.Length)).ToString());
            var ruleRep = escape(ToString().Substring(0, Math.Min(15, ToString().Length)));

            System.Diagnostics.Debug.WriteLine($"'{textExtract}' with '{ruleRep}' ");
#endif

            if (Interleave == null)
            {
                var result = OnMatch(text, trace);

#if DEBUG
                System.Diagnostics.Debug.WriteLine(result.IsSuccess ? " (Success)" : " (Fail)");
#endif

                return result;
            }
            else
            {
                //  Remove interleaves before
                int before = SeekInterleaves(text, trace);

                text = text.Skip(before);

                var result = OnMatch(text, trace);

                if (result.IsFailure)
                {
                    return result;
                }
                else
                {
                    //  Remove interleaves after
                    text = text.Skip(result.Match.MatchLength);

                    var after = SeekInterleaves(text, trace);

                    return new RuleResult(result.Match.IncreaseMatchLength(before + after));
                }
            }
        }

        protected abstract RuleResult OnMatch(SubString text, RuleTrace trace);

        protected int SeekInterleaves(SubString text, RuleTrace trace)
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
                    var result = Interleave.Match(text, trace);

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