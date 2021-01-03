using GramParserLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GramParserWorkbench.Models
{
    public static class AppVersionHelper
    {
        public static string ParserVersion => typeof(MetaGrammar).Assembly.GetName()!.Version!.ToString();
    }
}