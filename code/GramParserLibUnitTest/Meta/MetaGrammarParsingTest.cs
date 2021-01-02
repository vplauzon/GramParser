using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GramParserLib;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Immutable;

namespace GramParserLibUnitTest.Meta
{
    [TestClass]
    public class MetaGrammarParsingTest : MetaGrammarBaseTest
    {
        [TestMethod]
        public void ParsingString()
        {
            var ruleSet = MetaGrammar.ParseGrammar("rule main = \"a\";");

            Assert.IsNotNull(ruleSet);
        }

        [TestMethod]
        public void ParsingSequence()
        {
            var ruleSet = MetaGrammar.ParseGrammar("rule seq = \"a\" \"b\";");

            Assert.IsNotNull(ruleSet);
        }
    }
}