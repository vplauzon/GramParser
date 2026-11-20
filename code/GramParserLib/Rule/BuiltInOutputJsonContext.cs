using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace GramParserLib
{
    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(object[]))]
    [JsonSerializable(typeof(ImmutableArray<object>))]
    [JsonSerializable(typeof(IEnumerable<object>))]
    [JsonSerializable(typeof(ImmutableDictionary<string, object>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(SubString))]
    internal partial class BuiltInOutputJsonContext : JsonSerializerContext
    {
    }
}