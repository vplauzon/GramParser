using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using GramParserLib.Output;
using System.Text.Json;

namespace GramParserLib
{
    public class RuleMatch
    {
        private static readonly JsonSerializerOptions _defaultOptions = CreateDefaultOptions();

        private readonly Func<object?> _outputFactory;

        public RuleMatch(
            IRule rule,
            SubString text,
            Func<object?> outputFactory)
            : this(rule, text, text.Length, outputFactory)
        {
        }

        private RuleMatch(
            IRule rule,
            SubString text,
            int lengthWithInterleaves,
            Func<object?> outputFactory)
        {
            if (lengthWithInterleaves < text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthWithInterleaves));
            }

            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Text = text;
            LengthWithInterleaves = lengthWithInterleaves;
            _outputFactory = outputFactory;
        }

        public static RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        public IRule Rule { get; }

        public SubString Text { get; }

        public int LengthWithInterleaves { get; }

        public object? ComputeOutput()
        {
            var output = _outputFactory();

            return output;
        }

        public T ComputeTypedOutput<T>(JsonSerializerOptions? options = null)
        {
            var output = _outputFactory();
            var localOptions = new JsonSerializerOptions(options ?? _defaultOptions);

            localOptions.Converters.Add(SubString.JsonConverter);

            var serialized = JsonSerializer.Serialize(output, localOptions);
            var typedOutput = JsonSerializer.Deserialize<T>(serialized, localOptions);

            return typedOutput;
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
                    _outputFactory);
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

        private static JsonSerializerOptions CreateDefaultOptions()
        {
            var options = new JsonSerializerOptions();

            options.PropertyNameCaseInsensitive = true;

            return options;
        }
    }
}