using System.Text.Json.Serialization;

namespace GramParserLibUnitTest.TypedOutput
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(NestedOutput))]
    [JsonSerializable(typeof(CustomOutput))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
    internal partial class TestJsonContext : JsonSerializerContext
    {
    }
}