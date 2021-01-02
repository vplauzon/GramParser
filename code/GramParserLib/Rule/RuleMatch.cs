using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using GramParserLib.Output;

namespace GramParserLib
{
    public class RuleMatch
    {
        private readonly Func<object> _outputCreator;

        public RuleMatch(
            IRule rule,
            SubString text,
            Func<object> outputCreator)
            : this(rule, text, text.Length, outputCreator)
        {
        }

        private RuleMatch(
            IRule rule,
            SubString text,
            int lengthWithInterleaves,
            Func<object> outputCreator)
        {
            if (lengthWithInterleaves < text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthWithInterleaves));
            }

            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Text = text;
            LengthWithInterleaves = lengthWithInterleaves;
            _outputCreator = outputCreator;
        }

        public static RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        public IRule Rule { get; }

        public SubString Text { get; }

        public int LengthWithInterleaves { get; }

        public object ComputeOutput()
        {
            var output = _outputCreator();

            return output;
        }

        public RuleMatch AddInterleaveLength(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (length == 0)
            {
                return this;
            }
            else
            {
                return new RuleMatch(
                    Rule,
                    Text,
                    LengthWithInterleaves + length,
                    _outputCreator);
            }
        }

        #region object methods
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Rule.RuleName)
                ? Text.ToString()
                : $"<{Rule.RuleName}>{Text}";
        }
        #endregion

        private static object ExtractDefaultOutput(
            SubString text,
            IEnumerable<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            if (namedChildren != null && namedChildren.Count != 0)
            {
                var components = from c in namedChildren
                                 let output = c.Value.ComputeOutput()
                                 select KeyValuePair.Create(c.Key, output);
                var obj = ImmutableDictionary<string, object>.Empty.AddRange(components);

                return obj;
            }
            else if (children != null)
            {
                var outputs = from c in children
                              select c.ComputeOutput();
                var obj = outputs.ToArray();

                return obj;
            }
            else
            {
                return text.ToString();
            }
        }
    }
}