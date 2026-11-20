using GramParserLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GramParserLibUnitTest.TypedOutput
{
    [TestClass]
    public class TypedOutputSourceGenerationTest
    {
        [TestMethod]
        public void BuiltInNestedOutputsSerializeWithGeneratedMetadata()
        {
            var grammar = MetaGrammar.ParseGrammar(
                "rule root = a:\"a\"* b:\"b\"* => {\"a\":a, \"b\":b};");
            Assert.IsNotNull(grammar);
            var match = grammar!.Match("root", "aaabb");
            Assert.IsNotNull(match);

            var output = match!.ComputeTypedOutput<NestedOutput>(
                TestJsonContext.Default.NestedOutput);

            Assert.IsNotNull(output);
            CollectionAssert.AreEqual(new[] { "a", "a", "a" }, output!.A);
            CollectionAssert.AreEqual(new[] { "b", "b" }, output.B);
        }

        [TestMethod]
        public void BuiltInArrayOutputSerializesWithGeneratedMetadata()
        {
            var grammar = MetaGrammar.ParseGrammar("rule root = \"x\" => [text, text];");
            Assert.IsNotNull(grammar);
            var match = grammar!.Match("root", "x");
            Assert.IsNotNull(match);

            var output = match!.ComputeTypedOutput<string[]>(
                TestJsonContext.Default.StringArray);

            CollectionAssert.AreEqual(new[] { "x", "x" }, output);
        }

        [TestMethod]
        public void BuiltInPrependOutputSerializesWithGeneratedMetadata()
        {
            var grammar = MetaGrammar.ParseGrammar(
                "rule item = (\"a\"..\"z\")+ => text; "
                + "rule root = head:item tail:(\",\" i:item => i)* => prepend(head, tail);");
            Assert.IsNotNull(grammar);
            var match = grammar!.Match("root", "a,b,c");
            Assert.IsNotNull(match);

            var output = match!.ComputeTypedOutput<string[]>(
                TestJsonContext.Default.StringArray);

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, output);
        }

        [TestMethod]
        public void SubStringUsesItsJsonConverter()
        {
            var grammar = MetaGrammar.ParseGrammar("rule root = \"x\" => text;");
            Assert.IsNotNull(grammar);
            var match = grammar!.Match("root", "x");
            Assert.IsNotNull(match);

            var output = match!.ComputeTypedOutput<string>(TestJsonContext.Default.String);

            Assert.AreEqual("x", output);
        }

        [TestMethod]
        public void CustomOutputCanSupplySerializationMetadata()
        {
            var grammar = MetaGrammar.ParseGrammar("rule root = \"x\";");
            Assert.IsNotNull(grammar);
            var match = grammar!.Match("root", "x");
            Assert.IsNotNull(match);
            var customMatch = new RuleMatch(
                match!.Rule,
                match.Text,
                () => new CustomOutput { Name = "custom" });

            var output = customMatch.ComputeTypedOutput<CustomOutput>(
                TestJsonContext.Default.CustomOutput,
                TestJsonContext.Default.CustomOutput);

            Assert.IsNotNull(output);
            Assert.AreEqual("custom", output!.Name);
        }

        [TestMethod]
        public void NullOutputRemainsNull()
        {
            var grammar = MetaGrammar.ParseGrammar("rule root = \"x\" => null;");
            Assert.IsNotNull(grammar);
            var match = grammar!.Match("root", "x");
            Assert.IsNotNull(match);

            var output = match!.ComputeTypedOutput<NestedOutput>(
                TestJsonContext.Default.NestedOutput);

            Assert.IsNull(output);
        }
    }
}