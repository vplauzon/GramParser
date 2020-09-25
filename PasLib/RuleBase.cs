using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PasLib
{
    internal abstract class RuleBase : IRule
    {
        private readonly Lazy<IOutputExtractor> _outputExtractorFactory;

        protected RuleBase(
            string ruleName,
            Func<IOutputExtractor> outputExtractorFactory,
            bool? hasInterleave,
            bool? isRecursive,
            bool isTerminalRule,
            bool? hasChildrenDetails)
        {
            if (ruleName == string.Empty)
            {
                throw new ArgumentNullException(nameof(ruleName));
            }
            RuleName = ruleName;
            _outputExtractorFactory = new Lazy<IOutputExtractor>(
                outputExtractorFactory == null
                ? () => null
                : outputExtractorFactory);
            HasInterleave = hasInterleave;
            IsRecursive = isRecursive;
            IsTerminalRule = isTerminalRule;
            HasChildrenDetails = hasChildrenDetails;
        }

        #region IRuleProperties
        public bool? HasInterleave { get; }

        public bool? IsRecursive { get; }

        public bool IsTerminalRule { get; }

        public bool? HasChildrenDetails { get; }
        #endregion

        public string RuleName { get; private set; }

        public IEnumerable<RuleMatch> Match(ExplorerContext context)
        {
            return OnMatch(context);
        }

        public object ExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            var outputExtractor = _outputExtractorFactory.Value;

            if (outputExtractor != null)
            {
                return outputExtractor.ExtractOutput(text, children, namedChildren);
            }
            else
            {
                return DefaultExtractOutput(text, children, namedChildren);
            }
        }

        protected abstract IEnumerable<RuleMatch> OnMatch(ExplorerContext context);

        protected abstract object DefaultExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren);

        protected string ToString(IRule rule)
        {
            return string.IsNullOrWhiteSpace(rule.RuleName)
                ? rule.ToString()
                : rule.RuleName;
        }

        protected string ToStringRuleName()
        {
            return (string.IsNullOrWhiteSpace(RuleName) ? "" : $"<{RuleName}>");
        }
    }
}