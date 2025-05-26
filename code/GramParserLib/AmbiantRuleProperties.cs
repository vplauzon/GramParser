using System;
using System.Collections.Generic;
using System.Text;

namespace GramParserLib
{
    internal struct AmbiantRuleProperties
    {
        #region Constructors
        /// <summary>Initialize to default values.</summary>
        public AmbiantRuleProperties()
            : this(true, false, true)
        {
        }

        private AmbiantRuleProperties(
            bool hasInterleave,
            bool isTerminalRule,
            bool isCaseSensitive)
        {
            HasInterleave = hasInterleave;
            IsTerminalRule = isTerminalRule;
            IsCaseSensitive = isCaseSensitive;
        }
        #endregion

        public bool HasInterleave { get; }


        public bool IsTerminalRule { get; }
        
        public bool IsCaseSensitive { get; }

        public AmbiantRuleProperties Merge(IRuleProperties properties)
        {
            return new AmbiantRuleProperties(
                Merge(HasInterleave, properties.HasInterleave),
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