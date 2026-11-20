using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;

namespace GramParserLib
{
    public class RuleMatch
    {
        private static readonly JsonSerializerOptions _defaultOptions = CreateDefaultOptions();
        private static readonly BuiltInOutputJsonContext _outputJsonContext =
            new BuiltInOutputJsonContext(CreateDefaultOptions());

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

        public T? ComputeTypedOutput<T>(JsonSerializerOptions? options = null)
            where T : class
        {
            var output = _outputFactory();
            var serialized = JsonSerializer.Serialize(output, _defaultOptions);
            var typedOutput = JsonSerializer.Deserialize<T>(
                serialized,
                options ?? _defaultOptions);

            return typedOutput;
        }

        public T? ComputeTypedOutput<T>(JsonTypeInfo jsonTypeInfo)
            where T : class
        {
            var output = _outputFactory();
            var outputJsonTypeInfo = GetBuiltInOutputJsonTypeInfo(output);

            return ComputeTypedOutput<T>(
                output,
                jsonTypeInfo,
                outputJsonTypeInfo);
        }

        /// <summary>
        /// Computes typed output using explicit metadata for serialization and deserialization.
        /// </summary>
        /// <param name="jsonTypeInfo">Metadata for the requested typed output.</param>
        /// <param name="outputJsonTypeInfo">Metadata for the intermediate output value.</param>
        /// <returns>The typed output, or <see langword="null"/>.</returns>
        public T? ComputeTypedOutput<T>(
            JsonTypeInfo jsonTypeInfo,
            JsonTypeInfo outputJsonTypeInfo)
            where T : class
        {
            var output = _outputFactory();

            return ComputeTypedOutput<T>(
                output,
                jsonTypeInfo,
                outputJsonTypeInfo);
        }

        private static T? ComputeTypedOutput<T>(
            object? output,
            JsonTypeInfo jsonTypeInfo,
            JsonTypeInfo outputJsonTypeInfo)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);
            ArgumentNullException.ThrowIfNull(outputJsonTypeInfo);

            var serialized = JsonSerializer.Serialize(output, outputJsonTypeInfo);
            var typedOutput = JsonSerializer.Deserialize(serialized, jsonTypeInfo);

            return (T?)typedOutput;
        }

        private static JsonTypeInfo GetBuiltInOutputJsonTypeInfo(object? output)
        {
            var outputType = output?.GetType() ?? typeof(object);
            var jsonTypeInfo = _outputJsonContext.GetTypeInfo(outputType);

            if (jsonTypeInfo == null && output is IEnumerable<object?>)
            {
                jsonTypeInfo = _outputJsonContext.IEnumerableObject;
            }

            return jsonTypeInfo ?? throw new NotSupportedException(
                $"No built-in JSON metadata is available for output type '{outputType}'. "
                + "Pass serialization metadata for custom output types.");
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
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            options.Converters.Add(SubString.JsonConverter);

            return options;
        }
    }
}