using System;
using System.Collections.Generic;
using System.Text;

namespace GramParserLib
{
    internal class AmbiantRuleProperties
    {
        /// <summary>Initialize to default values.</summary>
        public AmbiantRuleProperties()
        {
            HasInterleave = true;
            IsRecursive = false;
            IsTerminalRule = false;
            IsCaseSensitive = true;
        }

        private AmbiantRuleProperties(
            bool hasInterleave,
            bool isRecursive,
            bool isTerminalRule,
            bool isCaseSensitive)
        {
            HasInterleave = hasInterleave;
            IsRecursive = isRecursive;
            IsTerminalRule = isTerminalRule;
            IsCaseSensitive = isCaseSensitive;
        }

        public bool HasInterleave { get; }

        public bool IsRecursive { get; }

        public bool IsTerminalRule { get; }
        
        public bool IsCaseSensitive { get; }

        public AmbiantRuleProperties Merge(IRuleProperties properties)
        {
            return new AmbiantRuleProperties(
                Merge(HasInterleave, properties.HasInterleave),
                Merge(IsRecursive, properties.IsRecursive),
                Merge(IsTerminalRule, properties.IsTerminalRule),
                Merge(IsCaseSensitive, properties.IsCaseSensitive));
        }

        private bool Merge(bool currentProperty, bool? newProperty)
        {
            return newProperty == null
                ? currentProperty
                : newProperty.Value;
        }
    }
}