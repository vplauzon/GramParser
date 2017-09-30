using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal class AmbiantRuleProperties
    {
        /// <summary>Initialize to default values.</summary>
        public AmbiantRuleProperties()
        {
            HasInterleave = false;
            IsRecursive = false;
            IsTerminalRule = false;
        }

        private AmbiantRuleProperties(
            bool hasInterleave,
            bool isRecursive,
            bool isTerminalRule)
        {
            HasInterleave = hasInterleave;
            IsRecursive = isRecursive;
            IsTerminalRule = isTerminalRule;
        }

        public bool HasInterleave { get; }

        public bool IsRecursive { get; }

        public bool IsTerminalRule { get; }

        public AmbiantRuleProperties Merge(IRuleProperties properties)
        {
            return new AmbiantRuleProperties(
                Merge(HasInterleave, properties.HasInterleave),
                Merge(IsRecursive, properties.IsRecursive),
                Merge(IsTerminalRule, properties.IsTerminalRule));
        }

        private bool Merge(bool currentProperty, bool? newProperty)
        {
            return newProperty == null
                ? currentProperty
                : newProperty.Value;
        }
    }
}