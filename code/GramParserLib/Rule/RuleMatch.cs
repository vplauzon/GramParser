using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using GramParserLib.Output;

namespace GramParserLib
{
    public class RuleMatch
    {
        public RuleMatch(IRule rule, SubString text)
            : this(rule, text, text.Length, null, null)
        {
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IEnumerable<RuleMatch> children)
            : this(rule, text, text.Length, children, null)
        {
        }

        public RuleMatch(
            IRule rule,
            SubString text,
            IImmutableDictionary<string, RuleMatch> namedChildren)
            : this(rule, text, text.Length, null, namedChildren)
        {
        }

        private RuleMatch(
            IRule rule,
            SubString text,
            int lengthWithInterleaves,
            IEnumerable<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            if (lengthWithInterleaves < text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthWithInterleaves));
            }
            if (children != null && namedChildren != null)
            {
                throw new ArgumentException(
                    $"Both {nameof(children)} and {nameof(namedChildren)} can't be non-null");
            }

            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Text = text;
            LengthWithInterleaves = lengthWithInterleaves;
            Children = children != null
                ? ImmutableList<RuleMatch>.Empty.AddRange(children)
                : ImmutableList<RuleMatch>.Empty;
            NamedChildren = namedChildren != null
                ? namedChildren
                : ImmutableDictionary<string, RuleMatch>.Empty;
        }

        public static RuleMatch[] EmptyMatch { get; } = new RuleMatch[0];

        public IRule Rule { get; }

        public SubString Text { get; }

        public int LengthWithInterleaves { get; }

        public IImmutableList<RuleMatch> Children { get; }
            = ImmutableList<RuleMatch>.Empty;

        public IImmutableDictionary<string, RuleMatch> NamedChildren { get; }
            = ImmutableDictionary<string, RuleMatch>.Empty;

        public object ComputeOutput()
        {
            var output = Rule.ExtractOutput(Text, Children, NamedChildren);

            return ExtractorHelper.StringAsString(output);
        }

        public RuleMatch ChangeRule(IRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (object.ReferenceEquals(rule, Rule))
            {
                return this;
            }
            else
            {
                return !NamedChildren.Any()
                    ? new RuleMatch(
                        rule,
                        Text,
                        LengthWithInterleaves,
                        Children,
                        null)
                    : new RuleMatch(
                        rule,
                        Text,
                        LengthWithInterleaves,
                        null,
                        NamedChildren);
            }
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
                return !NamedChildren.Any()
                    ? new RuleMatch(
                        Rule,
                        Text,
                        LengthWithInterleaves + length,
                        Children,
                        null)
                    : new RuleMatch(
                        Rule,
                        Text,
                        LengthWithInterleaves + length,
                        null,
                        NamedChildren);
            }
        }

        public RuleMatch RemoveChildren()
        {
            return !NamedChildren.Any() && !Children.Any()
                ? this
                : new RuleMatch(Rule, Text, LengthWithInterleaves, null, null);
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