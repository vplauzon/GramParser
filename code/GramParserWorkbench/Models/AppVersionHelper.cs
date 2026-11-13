using GramParserLib;
using System.Reflection;

namespace GramParserWorkbench.Models
{
    public static class AppVersionHelper
    {
        public static string ParserVersion =>
            typeof(MetaGrammar).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(MetaGrammar).Assembly.GetName().Version?.ToString()
            ?? "Unknown";
    }
}