using System;
using System.Collections.Generic;

namespace PasLib
{
    public abstract class TracePolicy
    {
        #region Inner Types
        private class NoTracePolicy : TracePolicy
        {
            internal override object CreateTrialAccumulator() => this;

            internal override void AddTrial(object accumulator, RuleResult result)
            {
                if (!object.ReferenceEquals(this, accumulator))
                {
                    throw new InvalidOperationException("accumulator is invalid");
                }
                //  Do nothing with result since we keep no trace
            }

            internal override RuleResult[] ExtractTrials(object accumulator)
            {
                if (!object.ReferenceEquals(this, accumulator))
                {
                    throw new InvalidOperationException("accumulator is invalid");
                }

                return EmptyTrials;
            }
        }

        private class TrialTreePolicy : TracePolicy
        {
            internal override object CreateTrialAccumulator() => new List<RuleResult>();

            internal override void AddTrial(object accumulator, RuleResult result)
            {
                var list = accumulator as List<RuleResult>;

                if (list == null)
                {
                    throw new InvalidOperationException("accumulator is invalid");
                }

                list.Add(result);
            }

            internal override RuleResult[] ExtractTrials(object accumulator)
            {
                var list = accumulator as List<RuleResult>;

                if (list == null)
                {
                    throw new InvalidOperationException("accumulator is invalid");
                }

                return list.ToArray();
            }
        }
        #endregion

        private static readonly RuleResult[] EMPTY = new RuleResult[0];

        public static TracePolicy NoTrace { get; private set; } = new NoTracePolicy();

        public static TracePolicy TrialTree { get; private set; } = new TrialTreePolicy();

        internal RuleResult[] EmptyTrials { get => EMPTY; }

        internal abstract object CreateTrialAccumulator();

        internal abstract void AddTrial(object accumulator, RuleResult result);

        internal abstract RuleResult[] ExtractTrials(object accumulator);
    }
}