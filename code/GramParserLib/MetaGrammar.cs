using GramParserLib.Output;
using GramParserLib.Rule;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GramParserLib
{
    public static class MetaGrammar
    {
        #region Inner Types
        private class GrammarCreator
        {
            #region Inner Types
            private class PropertyBag
            {
                public static PropertyBag Default { get; } = new PropertyBag();

                public bool? HasInterleave { get; set; }

                public bool? IsRecursive { get; set; }

                public bool IsDefault
                {
                    get
                    {
                        return !HasInterleave.HasValue && !IsRecursive.HasValue;
                    }
                }
            }
            #endregion

            private readonly Dictionary<string, IRule> _ruleMap =
                new Dictionary<string, IRule>();
            private readonly Dictionary<string, List<RuleProxy>> _proxies =
                new Dictionary<string, List<RuleProxy>>();

            public GrammarCreator(RuleMatch match)
            {
                IRule interleave = null;
                var declarations = ToList(match.ComputeOutput());

                foreach (var declaration in declarations)
                {
                    var declarationMap = ToMap(declaration);
                    var tag = declarationMap.Keys.First();
                    var subDeclaration = declarationMap.Values.First();

                    if (tag == "interleaveDeclaration")
                    {
                        var ruleBody = ToMap(ToMap(subDeclaration).First().Value);

                        interleave = CreateRule(
                            "#interleave",
                            PropertyBag.Default,
                            ruleBody,
                            null);
                    }
                    else if (tag == "ruleDeclaration")
                    {
                        var ruleDeclarationMap = ToMap(subDeclaration);
                        var ruleID = ((SubString)ruleDeclarationMap["id"]).ToString();
                        var parameterAssignationList = ToList(ruleDeclarationMap["params"]);
                        var ruleBodyOutputMap = ToMap(ruleDeclarationMap["rule"]);
                        var ruleBody = ToMap(ruleBodyOutputMap["body"]);
                        var outputList = ToList(ruleBodyOutputMap["output"]);
                        var outputDeclaration = outputList.FirstOrDefault();
                        var outputBodyMap = outputDeclaration != null
                            ? ToMap(ToMap(outputDeclaration)["output"])
                            : null;
                        var rule = CreateRule(ruleID, parameterAssignationList, ruleBody, outputBodyMap);

                        _ruleMap[ruleID] = rule;
                    }
                    else
                    {
                        throw new NotSupportedException($"Tag '{tag}' for declaration");
                    }
                }

                ResolveProxies();
                CreatedGrammar = new Grammar(_ruleMap, interleave);
            }

            private IImmutableList<object> ToList(object obj)
            {
                return (IImmutableList<object>)obj;
            }

            private IImmutableDictionary<string, object> ToMap(object obj)
            {
                return (IImmutableDictionary<string, object>)obj;
            }

            private void ResolveProxies()
            {
                foreach (var pair in _proxies)
                {
                    var ruleName = pair.Key;
                    var referenceList = pair.Value;

                    if (!_ruleMap.ContainsKey(ruleName))
                    {
                        throw new ParsingException(
                            $"Can't find definition of rule '{ruleName}'");
                    }

                    var rule = _ruleMap[ruleName];

                    foreach (var proxy in referenceList)
                    {
                        proxy.ReferencedRule = rule;
                    }
                }
            }

            public Grammar CreatedGrammar { get; }

            private TaggedRule CreateTaggedRule(
                IImmutableDictionary<string, object> tag,
                IRule rule)
            {
                var tagChild = tag.First();

                switch (tagChild.Key)
                {
                    case "noTag":
                        return new TaggedRule(rule);
                    case "withTag":
                        {
                            var id = ToMap(tagChild.Value);

                            if (id.Count() == 0)
                            {
                                return new TaggedRule(null, rule);
                            }
                            else
                            {
                                var idText = (SubString)id.First().Value;

                                return new TaggedRule(
                                    idText.Length == 0 ? null : idText.ToString(),
                                    rule);
                            }
                        }
                    default:
                        throw new NotSupportedException(
                            $"Tag of type {tagChild.Key} isn't supported");
                }
            }

            private IRule CreateRule(IImmutableDictionary<string, object> ruleBody)
            {
                return CreateRule(null, PropertyBag.Default, ruleBody, null);
            }

            private IRule CreateRule(
                string ruleID,
                IImmutableList<object> parameterAssignationList,
                IImmutableDictionary<string, object> ruleBody,
                IImmutableDictionary<string, object> outputBodyMap)
            {
                var propertyBag = CreatePropertyBag(parameterAssignationList);
                var outputExtractor = CreateOutputExtractor(outputBodyMap);

                return CreateRule(ruleID, propertyBag, ruleBody, outputExtractor);
            }

            private IRule CreateRule(
                string ruleID,
                PropertyBag propertyBag,
                IImmutableDictionary<string, object> ruleBody,
                IRuleOutput outputExtractor)
            {
                var ruleBodyTag = ruleBody.First().Key;
                var ruleBodyBody = ruleBody.First().Value;

                switch (ruleBodyTag)
                {
                    case "ruleRef":
                        return CreateRuleReference(ruleID, (SubString)ruleBodyBody, propertyBag);
                    case "literal":
                        return CreateLiteral(
                            ruleID, ToMap(ruleBodyBody), propertyBag, outputExtractor);
                    case "any":
                        return CreateAnyCharacter(ruleID, propertyBag, outputExtractor);
                    case "range":
                        return CreateRange(
                            ruleID, ToMap(ruleBodyBody), propertyBag, outputExtractor);
                    case "repeat":
                        return CreateRepeat(
                            ruleID, ToMap(ruleBodyBody), propertyBag, outputExtractor);
                    case "disjunction":
                        return CreateDisjunction(
                            ruleID, ToMap(ruleBodyBody), propertyBag, outputExtractor);
                    case "sequence":
                        return CreateSequence(
                            ruleID, ToMap(ruleBodyBody), propertyBag, outputExtractor);
                    case "substract":
                        return CreateSubstract(
                            ruleID, ToMap(ruleBodyBody), propertyBag, outputExtractor);
                    case "bracket":
                        return CreateBracket(
                            ruleID, ToMap(ruleBodyBody), propertyBag, outputExtractor);
                    default:
                        throw new NotSupportedException(ruleBodyTag);
                }
            }

            private PropertyBag CreatePropertyBag(IImmutableList<object> parameterAssignationList)
            {
                if (parameterAssignationList == null
                    || parameterAssignationList.Count == 0)
                {
                    return PropertyBag.Default;
                }
                else
                {
                    var list = ToMap(parameterAssignationList.First());
                    var head = list["head"];
                    var tail = ToList(list["tail"]);
                    var tailParamAssignations = from t in tail
                                                let tMap = ToMap(t)
                                                select tMap.First().Value;
                    var paramAssignations = tailParamAssignations.Append(head);
                    var bag = new PropertyBag();

                    foreach (var assignation in paramAssignations)
                    {
                        var assignationMap = ToMap(assignation);
                        var id = assignationMap["id"].ToString();
                        var value = assignationMap["value"].ToString();

                        switch (id)
                        {
                            case "interleave":
                                AssignInterleaveToBag(bag, value);
                                break;
                            case "recursive":
                                AssignRecursiveToBag(bag, value);
                                break;
                            default:
                                throw new ParsingException(
                                    $"Parameter '{id}' isn't supported");
                        }
                    }

                    return bag;
                }
            }

            private IRuleOutput CreateOutputExtractor(IImmutableDictionary<string, object> outputBodyMap)
            {
                if (outputBodyMap == null)
                {
                    return null;
                }
                else
                {
                    return CreateOutputExtractorFromBody(outputBodyMap);
                }
            }

            private IRuleOutput CreateOutputExtractorFromBody(IImmutableDictionary<string, object> outputBody)
            {
                var item = outputBody.First();
                var tagName = item.Key;
                var tagValue = item.Value;

                switch (tagName)
                {
                    case "id":
                        return CreateOutputExtractorFromId((SubString)tagValue);
                    case "literal":
                        return CreateOutputExtractorFromLiteral(ToMap(tagValue));
                    case "number":
                        return CreateOutputExtractorFromNumber(((SubString)tagValue).ToString());
                    case "array":
                        return CreateOutputExtractorFromArray(ToMap(tagValue));
                    case "object":
                        return CreateOutputExtractorFromObject(ToMap(tagValue));
                    case "function":
                        return CreateOutputExtractorFromFunction(ToMap(tagValue));
                    default:
                        throw new NotSupportedException($"Tag '{tagName}' not supported for output body");
                }
            }

            private IRuleOutput CreateOutputExtractorFromFunction(
                IImmutableDictionary<string, object> functionMap)
            {
                var functionName = functionMap["id"];
                var listChildren = ToList(functionMap["list"]);

                if (listChildren.Count == 0)
                {
                    return new FunctionOutput(
                        functionName.ToString(),
                        ImmutableArray<IRuleOutput>.Empty);
                }
                else
                {
                    var outputBodies = ExtractOutputBodiesFromArray(ToMap(listChildren.First()));
                    var extractors = from outputBody in outputBodies
                                     select CreateOutputExtractorFromBody(ToMap(outputBody));

                    return new FunctionOutput(functionName.ToString(), extractors);
                }
            }

            private IRuleOutput CreateOutputExtractorFromObject(
                IImmutableDictionary<string, object> objectMatch)
            {
                var listChildren = ToList(objectMatch["list"]);

                if (listChildren.Count == 0)
                {
                    return new ConstantOutput(new object());
                }
                else
                {
                    var pairs = ExtractPairsFromPairList(ToMap(listChildren.First()));

                    return new ObjectOutput(pairs);
                }
            }

            private IEnumerable<KeyValuePair<IRuleOutput, IRuleOutput>>
                ExtractPairsFromPairList(IImmutableDictionary<string, object> listMap)
            {
                var head = listMap["head"];
                var tail = ToList(listMap["tail"]);
                var tailElements = from e in tail
                                   let eMap = ToMap(e)
                                   select eMap.First().Value;
                var matchPairs = tailElements.Prepend(head);
                var extractorPairs = from p in matchPairs
                                     let pMap = ToMap(p)
                                     let keyMatch = ToMap(pMap["key"])
                                     let valueMatch = ToMap(pMap["value"])
                                     let key = CreateOutputExtractorFromBody(keyMatch)
                                     let value = CreateOutputExtractorFromBody(valueMatch)
                                     select KeyValuePair.Create(key, value);

                return extractorPairs;
            }

            private IRuleOutput CreateOutputExtractorFromArray(IImmutableDictionary<string, object> arrayMap)
            {
                var listChildren = ToList(arrayMap["list"]);

                if (listChildren.Count == 0)
                {
                    return new ConstantOutput(new object[0]);
                }
                else
                {
                    var outputBodies = ExtractOutputBodiesFromArray(ToMap(listChildren.First()));
                    var extractors = from outputBody in outputBodies
                                     let outputBodyMap = ToMap(outputBody)
                                     select CreateOutputExtractorFromBody(outputBodyMap);

                    return new ArrayOutput(extractors);
                }
            }

            private IEnumerable<object> ExtractOutputBodiesFromArray(IImmutableDictionary<string, object> listMap)
            {
                var head = listMap["head"];
                var tail = ToList(listMap["tail"]);
                var tailElements = from e in tail
                                   let eMap = ToMap(e)
                                   select eMap.First().Value;

                return Enumerable.Prepend(tailElements, head);
            }

            private IRuleOutput CreateOutputExtractorFromNumber(string text)
            {
                if (!text.Contains('.'))
                {
                    var value = int.Parse(text);

                    return new ConstantOutput(value);
                }
                else
                {
                    var value = double.Parse(text);

                    return new ConstantOutput(value);
                }
            }

            private IRuleOutput CreateOutputExtractorFromLiteral(IImmutableDictionary<string, object> literal)
            {
                var characters = from l in ToList(literal.First().Value)
                                 let lMap = ToMap(l)
                                 let c = GetCharacter(lMap)
                                 select c;

                return new ConstantOutput(new string(characters.ToArray()));
            }

            private IRuleOutput CreateOutputExtractorFromId(SubString id)
            {
                if (id.Equals("text"))
                {
                    return TextOutput.Instance;
                }
                else if (id.Equals("true"))
                {
                    return new ConstantOutput(true);
                }
                else if (id.Equals("false"))
                {
                    return new ConstantOutput(false);
                }
                else if (id.Equals("null"))
                {
                    return new ConstantOutput(null);
                }
                else
                {
                    return new ChildOutput(id.ToString());
                }
            }

            private static void AssignInterleaveToBag(PropertyBag bag, string value)
            {
                switch (value)
                {
                    case "true":
                        bag.HasInterleave = true;
                        break;
                    case "false":
                        bag.HasInterleave = false;
                        break;
                    default:
                        throw new ParsingException(
                            $"Value '{value}' isn't supported for interleave parameter");
                }
            }

            private static void AssignRecursiveToBag(PropertyBag bag, string value)
            {
                switch (value)
                {
                    case "true":
                        bag.IsRecursive = true;
                        break;
                    case "false":
                        bag.IsRecursive = false;
                        break;
                    default:
                        throw new ParsingException(
                            $"Value '{value}' isn't supported for recursive parameter");
                }
            }

            private IRule CreateRuleReference(
                string ruleID,
                SubString ruleName,
                PropertyBag propertyBag)
            {
                if (!propertyBag.IsDefault)
                {
                    throw new ParsingException("Rule reference can't have parameters");
                }

                var identifier = ruleName.ToString();

                //  If the referenced rule has already been parsed we insert it
                //  Otherwise, we put a proxy
                if (_ruleMap.ContainsKey(identifier))
                {
                    var rule = _ruleMap[identifier];

                    return rule;
                }
                else
                {
                    var rule = new RuleProxy();

                    if (!_proxies.ContainsKey(identifier))
                    {
                        _proxies[identifier] = new List<RuleProxy>();
                    }

                    var referenceList = _proxies[identifier];

                    //  Store the proxy for later resolving it
                    referenceList.Add(rule);

                    return rule;
                }
            }

            private IRule CreateLiteral(
                string ruleID,
                IImmutableDictionary<string, object> ruleBodyBodyMap,
                PropertyBag propertyBag,
                IRuleOutput outputExtractor)
            {
                if (ruleBodyBodyMap is null)
                {
                    throw new ArgumentNullException(nameof(ruleBodyBodyMap));
                }

                var literal = ToList(ruleBodyBodyMap.First().Value);
                var characters = from l in literal
                                 let lMap = ToMap(l)
                                 let c = GetCharacter(lMap)
                                 select c;
                var rule = new LiteralRule(ruleID, outputExtractor, characters);

                return rule;
            }

            private char GetCharacter(IImmutableDictionary<string, object> character)
            {
                var charFragment = character.First();
                var subMatch = charFragment.Value;

                switch (charFragment.Key)
                {
                    case "normal":
                        return ((SubString)subMatch).First();
                    case "escapeLetter":
                        return GetEscapeLetter(((SubString)ToMap(subMatch).First().Value).First());
                    case "escapeQuote":
                        return '\"';
                    case "escapeBackslash":
                        return '\\';
                    case "escapeHexa":
                        return (char)int.Parse(
                            //  Skip the \x
                            ((SubString)subMatch).ToString().Substring(2),
                            NumberStyles.HexNumber);
                    default:
                        throw new NotSupportedException(
                            $"Character tag not supported:  '{charFragment.Key}'");
                }
            }

            private char GetEscapeLetter(char letter)
            {
                switch (letter)
                {
                    case 'n':
                        return '\n';
                    case 'r':
                        return '\r';
                    case 't':
                        return '\t';
                    case 'v':
                        return '\v';
                    default:
                        throw new ParsingException(
                            $"Character escape not supported:  \\{letter}");
                }
            }

            private IRule CreateAnyCharacter(
                string ruleID,
                PropertyBag propertyBag,
                IRuleOutput outputExtractor)
            {
                return new MatchAnyCharacterRule(ruleID, outputExtractor);
            }

            private IRule CreateRange(
                string ruleID,
                IImmutableDictionary<string, object> ruleBodyBody,
                PropertyBag propertyBag,
                IRuleOutput outputExtractor)
            {
                var lower = ToMap(ToMap(ruleBodyBody["lower"]).First().Value);
                var upper = ToMap(ToMap(ruleBodyBody["upper"]).First().Value);
                var lowerChar = GetCharacter(lower);
                var upperChar = GetCharacter(upper);

                return new RangeRule(ruleID, outputExtractor, lowerChar, upperChar);
            }

            private IRule CreateRepeat(
                string ruleID,
                IImmutableDictionary<string, object> ruleBodyBody,
                PropertyBag bag,
                IRuleOutput outputExtractor)
            {
                var subRuleBody = ToMap(ruleBodyBody["rule"]);
                var cardinality = ToMap(ruleBodyBody["cardinality"]);
                var rule = CreateRule(subRuleBody);

                switch (cardinality.First().Key)
                {
                    case "star":
                        return new RepeatRule(
                            ruleID,
                            outputExtractor,
                            rule,
                            null,
                            null,
                            bag.HasInterleave,
                            bag.IsRecursive);
                    case "plus":
                        return new RepeatRule(
                            ruleID,
                            outputExtractor,
                            rule,
                            1,
                            null,
                            bag.HasInterleave,
                            bag.IsRecursive);
                    case "question":
                        return new RepeatRule(
                            ruleID,
                            outputExtractor,
                            rule,
                            0,
                            1,
                            bag.HasInterleave,
                            bag.IsRecursive);
                    case "exact":
                        {
                            var exact = ToMap(cardinality.First().Value);
                            var n = int.Parse(exact.First().Value.ToString());

                            return new RepeatRule(
                                ruleID,
                                outputExtractor,
                                rule,
                                n,
                                n,
                                bag.HasInterleave,
                                bag.IsRecursive);
                        }
                    case "minMax":
                        {
                            var minMaxCardinality = cardinality.First().Value;
                            (var min, var max) = GetMinMaxCardinality(ToMap(minMaxCardinality));

                            return new RepeatRule(
                                ruleID,
                                outputExtractor,
                                rule,
                                min,
                                max,
                                bag.HasInterleave,
                                bag.IsRecursive);
                        }
                    default:
                        throw new NotSupportedException();
                }
            }

            private (int? min, int? max) GetMinMaxCardinality(
                IImmutableDictionary<string, object> minMaxCardinality)
            {
                var type = minMaxCardinality.First().Key;
                var cardinality = ToMap(minMaxCardinality.First().Value);

                switch (type)
                {
                    case "minmax":
                        {
                            var minText = cardinality["min"].ToString();
                            var maxText = cardinality["max"].ToString();
                            var min = int.Parse(minText);
                            var max = int.Parse(maxText);

                            return (min, max);
                        }
                    case "min":
                        {
                            var minText = cardinality["min"].ToString();
                            var min = int.Parse(minText);

                            return (min, null);
                        }
                    case "max":
                        {
                            var maxText = cardinality["max"].ToString();
                            var max = int.Parse(maxText);

                            return (null, max);
                        }
                    default:
                        throw new NotImplementedException($"Cardinality:  '{type}'");
                }
            }

            private IRule CreateDisjunction(
                string ruleID,
                IImmutableDictionary<string, object> ruleBodyBody,
                PropertyBag bag,
                IRuleOutput outputExtractor)
            {
                var headTag = ToMap(ruleBodyBody["t"]);
                var head = ToMap(ruleBodyBody["head"]);
                var tail = ToList(ruleBodyBody["tail"]);
                var headRule = CreateTaggedRule(headTag, CreateRule(head));
                var tailRules = from c in tail
                                let cMap = ToMap(c)
                                let tailTag = ToMap(cMap["t"])
                                let tailDisjunctable = ToMap(cMap["d"])
                                select CreateTaggedRule(
                                    tailTag,
                                    CreateRule(tailDisjunctable));
                var rules = new[] { headRule }.Concat(tailRules);

                return new DisjunctionRule(
                    ruleID,
                    outputExtractor,
                    rules,
                    bag.HasInterleave,
                    bag.IsRecursive);
            }

            private IRule CreateSequence(
                string ruleID,
                IImmutableDictionary<string, object> ruleBodyBody,
                PropertyBag bag,
                IRuleOutput outputExtractor)
            {
                var head = ruleBodyBody["head"];
                var tail = ToList(ruleBodyBody["tail"]);
                var tailInner = from s in tail
                                let sMap = ToMap(s)
                                select sMap["s"];
                var rules = from tagRule in tailInner.Prepend(head)
                            let tagRuleMap = ToMap(tagRule)
                            let t = ToMap(tagRuleMap["t"])
                            let r = ToMap(tagRuleMap["r"])
                            let rule = CreateRule(r)
                            select CreateTaggedRule(t, rule);

                return new SequenceRule(
                    ruleID,
                    outputExtractor,
                    rules,
                    bag.HasInterleave,
                    bag.IsRecursive);
            }

            private IRule CreateSubstract(
                string ruleID,
                IImmutableDictionary<string, object> ruleBodyBody,
                PropertyBag bag,
                IRuleOutput outputExtractor)
            {
                var primary = ToMap(ruleBodyBody["primary"]);
                var excluded = ToMap(ruleBodyBody["excluded"]);
                var primaryRule = CreateRule(primary);
                var excludedRule = CreateRule(excluded);

                return new SubstractRule(
                    ruleID,
                    outputExtractor,
                    primaryRule,
                    excludedRule,
                    bag.HasInterleave,
                    bag.IsRecursive);
            }

            private IRule CreateBracket(
                string ruleID,
                IImmutableDictionary<string, object> ruleBodyBody,
                PropertyBag bag,
                IRuleOutput outputExtractor)
            {
                var ruleBodyOutput = ToMap(ruleBodyBody.First().Value);
                var ruleBody = ToMap(ruleBodyOutput["body"]);
                var outputList = ToList(ruleBodyOutput["output"]);
                var outputDeclaration = outputList.FirstOrDefault();
                var outputBodyMap = outputDeclaration != null
                    ? ToMap(ToMap(outputDeclaration)["output"])
                    : null;
                var innerOutputExtractor = CreateOutputExtractor(outputBodyMap);
                var innerRule = CreateRule(null, bag, ruleBody, innerOutputExtractor);

                if (outputExtractor != null)
                {
                    var outerRule = new SequenceRule(
                        ruleID,
                        outputExtractor,
                        new[] { new TaggedRule(innerRule) },
                        bag.HasInterleave,
                        bag.IsRecursive);

                    return outerRule;
                }
                else
                {
                    return innerRule;
                }
            }
        }
        #endregion

        private const string MAIN_RULE = "main";

        private static readonly Grammar _metaSet = CreateMetaGrammar();

        public static Grammar ParseGrammar(SubString text)
        {
            var match = _metaSet.Match(MAIN_RULE, text);

            if (match != null)
            {
                var grammarCreator = new GrammarCreator(match);
                var grammar = grammarCreator.CreatedGrammar;

                return grammar;
            }
            else
            {
                return null;
            }
        }

        private static Grammar CreateMetaGrammar()
        {
            //  Comments & interleaves
            var carriageReturn = new DisjunctionRule("#carriageReturn", null, TaggedRule.FromRules(
                new LiteralRule(null, null, "\r"),
                new LiteralRule(null, null, "\n")), false, false);
            var commentContentChar = new SubstractRule(
                "#commentContentChar",
                null,
                new MatchAnyCharacterRule(null, null),
                carriageReturn,
                false);
            var commentContent =
                new RepeatRule(null, null, commentContentChar, null, null, false, false);
            var comment = new SequenceRule(
                "comment",
                null,
                TaggedRule.FromRules(
                    new LiteralRule(null, null, "#"),
                    commentContent),
                false,
                false);
            var interleave = new DisjunctionRule(
                "$interleave$",
                null,
                TaggedRule.FromRules(
                    new LiteralRule(null, null, " "),
                    new LiteralRule(null, null, "\r"),
                    new LiteralRule(null, null, "\n"),
                    new LiteralRule(null, null, "\t"),
                    comment),
                false,
                false);
            //  Tokens
            var identifierPrefixChar = new DisjunctionRule(
                null,
                null,
                TaggedRule.FromRules(
                    new RangeRule(null, null, 'a', 'z'),
                    new RangeRule(null, null, 'A', 'Z')),
                false,
                false);
            var identifierSuffixChar = new DisjunctionRule(
                null,
                null,
                TaggedRule.FromRules(
                    new RangeRule(null, null, 'a', 'z'),
                    new RangeRule(null, null, 'A', 'Z'),
                    new RangeRule(null, null, '0', '9')),
                false,
                false);
            var identifier = new SequenceRule(
                "identifier",
                TextOutput.Instance,
                new[]
                {
                    new TaggedRule(identifierPrefixChar),
                    new TaggedRule(
                        new RepeatRule(
                            null,
                            null,
                            identifierSuffixChar,
                            null,
                            null,
                            false,
                            false))
                },
                false,
                false);
            var number = new RepeatRule(
                "number",
                TextOutput.Instance,
                new RangeRule(null, null, '0', '9'),
                1,
                null,
                false,
                false);
            var doubleRule = new SequenceRule(
                "double",
                TextOutput.Instance,
                new[]
                {
                    new TaggedRule(
                        null,
                        new RepeatRule(null, null, new LiteralRule(null, null, "-"), null, 1)),
                    new TaggedRule(
                        null,
                        new RepeatRule(null, null, number, null, null)),
                    new TaggedRule(
                        null,
                        new RepeatRule(
                            null,
                            null,
                            new SequenceRule(
                                null,
                                null,
                                new[]
                                {
                                    new TaggedRule(new LiteralRule(null, null, ".")),
                                    new TaggedRule(new RepeatRule(null, null, number, 1, null))
                                }),
                            0,
                            1))
                },
                false);
            var character = GetCharacterRule();
            var quotedCharacter = new SequenceRule(
                "quotedCharacter",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "\"")),
                    new TaggedRule("l", character),
                    new TaggedRule(new LiteralRule(null, null, "\""))
                },
                false,
                false);

            //  Rules
            var withTag = new SequenceRule(
                "withTag",
                null,
                new[]
                {
                    new TaggedRule("id", identifier),
                    new TaggedRule(new LiteralRule(null, null, ":"))
                },
                false,
                false);
            var noTag = new LiteralRule("noTag", null, string.Empty);
            var tag = new DisjunctionRule(
                "tag",
                null,
                new[]
                {
                    new TaggedRule("withTag", withTag),
                    new TaggedRule("noTag", noTag)
                },
                false,
                false);
            var literal = new SequenceRule(
                "literal",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "\"")),
                    new TaggedRule("l", new RepeatRule(null, null, character, null, null)),
                    new TaggedRule(new LiteralRule(null, null, "\""))
                },
                false,
                false);
            var any = new LiteralRule("any", null, ".");
            var range = new SequenceRule(
                "range",
                null,
                new[]
                {
                    new TaggedRule("lower", quotedCharacter),
                    new TaggedRule(new RepeatRule(null, null, new LiteralRule(null, null, "."),2,2)),
                    new TaggedRule("upper", quotedCharacter)
                },
                false,
                false);
            var exactCardinality = new SequenceRule(
                "exactCardinality",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "{")),
                    new TaggedRule("n", number),
                    new TaggedRule(new LiteralRule(null, null, "}"))
                },
                hasInterleave: true);
            var minMaxCardinality = new DisjunctionRule(
                "minMaxCardinality",
                null,
                new[]
                {
                    new TaggedRule(
                        "minmax",
                        new SequenceRule(
                            "#minmax",
                            null,
                            new[]
                            {
                                new TaggedRule(new LiteralRule(null, null, "{")),
                                new TaggedRule("min", number),
                                new TaggedRule(new LiteralRule(null, null, ",")),
                                new TaggedRule("max", number),
                                new TaggedRule(new LiteralRule(null, null, "}"))
                            },
                            isRecursive: false)),
                    new TaggedRule(
                        "min",
                        new SequenceRule(
                            "#minmax",
                            null,
                            new[]
                            {
                                new TaggedRule(new LiteralRule(null, null, "{")),
                                new TaggedRule("min", number),
                                new TaggedRule(new LiteralRule(null, null, ",")),
                                new TaggedRule(new LiteralRule(null, null, "}"))
                            },
                            isRecursive: false)),
                    new TaggedRule(
                        "max",
                        new SequenceRule(
                            "#minmax",
                            null,
                            new[]
                            {
                                new TaggedRule(new LiteralRule(null, null, "{")),
                                new TaggedRule(new LiteralRule(null, null, ",")),
                                new TaggedRule("max", number),
                                new TaggedRule(new LiteralRule(null, null, "}"))
                            },
                            isRecursive: false))
                },
                hasInterleave: true);
            var cardinality = new DisjunctionRule(
                "cardinality",
                null,
                new[]
                {
                    new TaggedRule("star", new LiteralRule(null, null, "*")),
                    new TaggedRule("plus", new LiteralRule(null, null, "+")),
                    new TaggedRule("question", new LiteralRule(null, null, "?")),
                    new TaggedRule("exact", exactCardinality),
                    new TaggedRule("minMax", minMaxCardinality)
                },
                hasInterleave: true,
                isRecursive: false);
            var ruleBodyOutputProxy = new RuleProxy();
            var bracket = new SequenceRule(
                "bracket",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "(")),
                    new TaggedRule("r", ruleBodyOutputProxy),
                    new TaggedRule(new LiteralRule(null, null, ")"))
                },
                hasInterleave: true);
            var repeatable = new DisjunctionRule(
                "repeatable",
                null,
                new[]
                {
                    new TaggedRule("ruleRef", identifier),
                    new TaggedRule("literal", literal),
                    new TaggedRule("bracket", bracket),
                    new TaggedRule("any", any)
                },
                hasInterleave: true,
                isRecursive: false);
            var repeat = new SequenceRule(
                "repeat",
                null,
                new[]
                {
                    new TaggedRule("rule", repeatable),
                    new TaggedRule("cardinality", cardinality)
                },
                hasInterleave: true,
                isRecursive: false);
            var disjunctionable = new DisjunctionRule(
                "disjunctionable",
                null,
                new[]
                {
                    new TaggedRule("ruleRef", identifier),
                    new TaggedRule("literal", literal),
                    new TaggedRule("range", range),
                    new TaggedRule("bracket", bracket),
                    new TaggedRule("any", any),
                    new TaggedRule("repeat", repeat)
                },
                hasInterleave: true,
                isRecursive: false);
            var disjunction = new SequenceRule(
                "disjunction",
                null,
                new[]
                {
                    new TaggedRule("t", tag),
                    new TaggedRule("head", disjunctionable),
                    new TaggedRule("tail", new RepeatRule(
                        null,
                        null,
                        new SequenceRule(
                            null,
                            null,
                            new[]
                            {
                                new TaggedRule(new LiteralRule(null, null, "|")),
                                new TaggedRule("t", tag),
                                new TaggedRule("d", disjunctionable)
                            }),
                        1,
                        null))
                },
                hasInterleave: true,
                isRecursive: false);
            var sequenceable = new DisjunctionRule(
                "sequenceable",
                null,
                new[]
                {
                    new TaggedRule("ruleRef", identifier),
                    new TaggedRule("literal", literal),
                    new TaggedRule("range", range),
                    new TaggedRule("bracket", bracket),
                    new TaggedRule("any", any),
                    new TaggedRule("repeat", repeat)
                },
                hasInterleave: true,
                isRecursive: false);
            var innerSequenceable = new SequenceRule(
                "innerSequenceable",
                null,
                new[]
                {
                    new TaggedRule("t", tag),
                    new TaggedRule("r", sequenceable)
                },
                hasInterleave: false,
                isRecursive: false);
            var tailSequenceable = new SequenceRule(
                "tailSequenceable",
                null,
                new[]
                {
                    new TaggedRule(new RepeatRule(null, null, interleave, 1, null, false, false)),
                    new TaggedRule("s", innerSequenceable)
                },
                hasInterleave: false,
                isRecursive: false);
            var sequence = new SequenceRule(
                "sequence",
                null,
                new[]
                {
                    new TaggedRule("head", innerSequenceable),
                    new TaggedRule(
                        "tail",
                        new RepeatRule(null, null, tailSequenceable, 1, null, hasInterleave:false))
                },
                hasInterleave: false,
                isRecursive: false);
            var substractable = new DisjunctionRule(
                "substractable",
                null,
                new[]
                {
                    new TaggedRule("ruleRef", identifier),
                    new TaggedRule("literal", literal),
                    new TaggedRule("range", range),
                    new TaggedRule("bracket", bracket),
                    new TaggedRule("any", any),
                    new TaggedRule("repeat", repeat)
                },
                hasInterleave: true,
                isRecursive: false);
            var substracted = new DisjunctionRule(
                "substracted",
                null,
                new[]
                {
                    new TaggedRule("ruleRef", identifier),
                    new TaggedRule("literal", literal),
                    new TaggedRule("range", range),
                    new TaggedRule("bracket", bracket),
                    new TaggedRule("repeat", repeat)
                },
                isRecursive: false);
            var substract = new SequenceRule(
                "substract",
                null,
                new[]
                {
                    new TaggedRule("primary", substractable),
                    new TaggedRule(new LiteralRule(null, null, "-")),
                    new TaggedRule("excluded", substracted)
                },
                hasInterleave: true,
                isRecursive: false);
            var ruleBody = new DisjunctionRule(
                "ruleBody",
                null,
                new[]
                {
                    new TaggedRule("ruleRef", identifier),
                    new TaggedRule("literal", literal),
                    new TaggedRule("range", range),
                    new TaggedRule("bracket", bracket),
                    new TaggedRule("any", any),
                    new TaggedRule("substract", substract),
                    new TaggedRule("disjunction", disjunction),
                    new TaggedRule("repeat", repeat),
                    new TaggedRule("sequence", sequence)
                },
                hasInterleave: true);

            //  Outputs
            var outputBodyProxy = new RuleProxy();
            var outputArrayList = new SequenceRule(
                "outputArrayList",
                null,
                new[]
                {
                    new TaggedRule("head", outputBodyProxy),
                    new TaggedRule(
                        "tail",
                        new RepeatRule(
                            null,
                            null,
                            new SequenceRule(
                                null,
                                null,
                                new []
                                {
                                    new TaggedRule(new LiteralRule(null, null, ",")),
                                    new TaggedRule("element", outputBodyProxy)
                                }),
                            null,
                            null))
                },
                hasInterleave: true);
            var outputArray = new SequenceRule(
                "outputArray",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "[")),
                    new TaggedRule(
                        "list",
                        new RepeatRule(
                            null,
                            null,
                            outputArrayList,
                            null,
                            1)),
                    new TaggedRule(new LiteralRule(null, null, "]"))
                },
                hasInterleave: true);
            var outputObjectFieldKey = new DisjunctionRule(
                "outputObjectFieldKey",
                null,
                new[]
                {
                    new TaggedRule("id", identifier),
                    new TaggedRule("literal", literal)
                },
                hasInterleave: true);
            var outputObjectFieldPair = new SequenceRule(
                "outputObjectFieldPair",
                null,
                new[]
                {
                    new TaggedRule("key", outputObjectFieldKey),
                    new TaggedRule(new LiteralRule(null, null, ":")),
                    new TaggedRule("value", outputBodyProxy)
                },
                hasInterleave: true);
            var outputObjectFieldList = new SequenceRule(
                "outputObjectFieldList",
                null,
                new[]
                {
                    new TaggedRule("head", outputObjectFieldPair),
                    new TaggedRule(
                        "tail",
                        new RepeatRule(
                            null,
                            null,
                            new SequenceRule(
                                null,
                                null,
                                new []
                                {
                                    new TaggedRule(new LiteralRule(null, null, ",")),
                                    new TaggedRule("element", outputObjectFieldPair)
                                }),
                            null,
                            null))
                },
                hasInterleave: true);
            var outputObject = new SequenceRule(
                "outputObject",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "{")),
                    new TaggedRule(
                        "list",
                        new RepeatRule(
                            null,
                            null,
                            outputObjectFieldList,
                            null,
                            1)),
                    new TaggedRule(new LiteralRule(null, null, "}"))
                },
                hasInterleave: true);
            var outputFunction = new SequenceRule(
                "outputFunction",
                null,
                new[]
                {
                    new TaggedRule("id", identifier),
                    new TaggedRule(new LiteralRule(null, null, "(")),
                    new TaggedRule(
                        "list",
                        new RepeatRule(
                            null,
                            null,
                            outputArrayList,
                            null,
                            1)),
                    new TaggedRule(new LiteralRule(null, null, ")"))
                },
                hasInterleave: true);
            var outputBody = new DisjunctionRule(
                "outputBody",
                null,
                new[]
                {
                    new TaggedRule("id", identifier),
                    new TaggedRule("literal", literal),
                    new TaggedRule("number", doubleRule),
                    new TaggedRule("array", outputArray),
                    new TaggedRule("object", outputObject),
                    new TaggedRule("function", outputFunction)
                },
                hasInterleave: true,
                isRecursive: true);
            var outputDeclaration = new SequenceRule(
                "outputDeclaration",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "=>")),
                    new TaggedRule("output", outputBody)
                },
                hasInterleave: true);
            var ruleBodyOutput = new SequenceRule(
                "ruleBodyOutput",
                null,
                new[]
                {
                    new TaggedRule("body", ruleBody),
                    new TaggedRule(
                        "output",
                        new RepeatRule(null, null, outputDeclaration, 0, 1))
                },
                hasInterleave: true);

            //  Rule declarations
            var interleaveDeclaration = new SequenceRule(
                "interleaveDeclaration",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "interleave")),
                    new TaggedRule(new LiteralRule(null, null, "=")),
                    new TaggedRule("body", ruleBody),
                    new TaggedRule(new LiteralRule(null, null, ";"))
                });
            var boolean = new DisjunctionRule(
                "boolean",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule("true", null, "true")),
                    new TaggedRule(new LiteralRule("false", null, "false"))
                });
            var parameterAssignation = new SequenceRule(
                "parameterAssignation",
                null,
                new[]
                {
                    new TaggedRule("id", identifier),
                    new TaggedRule(new LiteralRule(null, null, "=")),
                    new TaggedRule("value", boolean),
                });
            var innerParameterAssignationList = new SequenceRule(
                null,
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, ",")),
                    new TaggedRule("pa", parameterAssignation)
                });
            var parameterAssignationList = new SequenceRule(
                "parameterAssignationList",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "(")),
                    new TaggedRule("head", parameterAssignation),
                    new TaggedRule("tail", new RepeatRule(
                        null,
                        null,
                        innerParameterAssignationList,
                        null,
                        null)),
                    new TaggedRule(new LiteralRule(null, null, ")"))
                });
            var ruleDeclaration = new SequenceRule(
                "ruleDeclaration",
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "rule")),
                    new TaggedRule(
                        "params",
                        new RepeatRule(null, null, parameterAssignationList, 0, 1)),
                    new TaggedRule("id", identifier),
                    new TaggedRule(new LiteralRule(null, null, "=")),
                    new TaggedRule("rule", ruleBodyOutput),
                    new TaggedRule(new LiteralRule(null, null, ";"))
                });
            var declaration = new DisjunctionRule(
                "declaration",
                null,
                new[]
                {
                    new TaggedRule("interleaveDeclaration", interleaveDeclaration),
                    new TaggedRule("ruleDeclaration", ruleDeclaration)
                });

            //  Main rule
            var main = new RepeatRule("main", null, declaration, 1, null, isRecursive: false);

            ruleBodyOutputProxy.ReferencedRule = ruleBodyOutput;
            outputBodyProxy.ReferencedRule = outputBody;

            return new Grammar(
                new Dictionary<string, IRule>() { { main.RuleName, main } },
                interleave);
        }

        private static IRule GetCharacterRule()
        {
            var normal = new SubstractRule(
                null,
                null,
                new MatchAnyCharacterRule(null, null),
                new DisjunctionRule(
                    null,
                    null,
                    TaggedRule.FromRules(new[]
                    {
                        new LiteralRule(null, null, "\""),
                        new LiteralRule(null, null, "\r"),
                        new LiteralRule(null, null, "\n"),
                        new LiteralRule(null, null, "\\")
                    }),
                    false,
                    false),
                false,
                false);
            var escapeQuote = new LiteralRule(null, null, "\\\"");
            var escapeBackslash = new LiteralRule(null, null, "\\\\");
            var escapeLetter = new SequenceRule(
                null,
                null,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "\\")),
                    new TaggedRule("l", new DisjunctionRule(null, null, TaggedRule.FromRules(new[]
                    {
                        new LiteralRule(null, null, "n"),
                        new LiteralRule(null, null, "r"),
                        new LiteralRule(null, null, "t"),
                        new LiteralRule(null, null, "v")
                    })))
                },
                false,
                false);
            var escapeHexa = new SequenceRule(
                null,
                TextOutput.Instance,
                new[]
                {
                    new TaggedRule(new LiteralRule(null, null, "\\x")),
                    new TaggedRule(
                        "h",
                        new RepeatRule(
                            null,
                            null,
                            new DisjunctionRule(
                                null,
                                null,
                                TaggedRule.FromRules(new[]
                                {
                                    new RangeRule(null, null, '0', '9'),
                                    new RangeRule(null, null, 'a', 'f'),
                                    new RangeRule(null, null, 'A', 'F')
                                }),
                                false,
                                false),
                            2,
                            2,
                            false,
                            false))
                },
                false,
                false);
            var character = new DisjunctionRule(
                "character",
                null,
                new[]
                {
                    new TaggedRule("normal", normal),
                    new TaggedRule("escapeQuote", escapeQuote),
                    new TaggedRule("escapeBackslash", escapeBackslash),
                    new TaggedRule("escapeLetter", escapeLetter),
                    new TaggedRule("escapeHexa", escapeHexa)
                },
                false,
                false);

            return character;
        }
    }
}