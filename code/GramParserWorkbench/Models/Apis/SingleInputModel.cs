using System;
using System.Collections.Generic;

namespace GramParserWorkbench.Models.Apis
{
    public class SingleInputModel
    {
        public string? Grammar { get; set; }

        public string? Rule { get; set; }

        public string? Text { get; set; }
    }
}