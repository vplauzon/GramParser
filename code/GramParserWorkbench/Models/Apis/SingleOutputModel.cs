using GramParserLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GramParserWorkbench.Models.Apis
{
    internal class SingleOutputModel
    {
        public SingleOutputModel(
            RuleMatch? match,
            TimeSpan grammarDuration,
            TimeSpan matchDuration)
        {
            IsMatch = match != null;
            if (match != null)
            {
                Rule = match.Rule.RuleName;
                Text = match.Text.ToString();
                //  To force SubString to be serialized into strings
                Output = match.ComputeTypedOutput<object>();
            }

            Duration = new DurationModel(grammarDuration, matchDuration);
        }

        public string ApiVersion => AppVersionHelper.ParserVersion;

        public bool IsMatch { get; set; }

        public DurationModel Duration { get; set; }

        public string? Rule { get; }

        public string? Text { get; }

        public object? Output { get; }
    }
}