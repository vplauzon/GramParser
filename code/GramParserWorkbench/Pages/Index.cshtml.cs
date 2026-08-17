using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GramParserWorkbench.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GramParserWorkbench.Pages
{
    public class IndexModel : PageModel
    {
        public string ParsingApiUrl { get; set; }

        public string ParserVersion => AppVersionHelper.ParserVersion;

        public string WorkbenchEnvironment => Environment.GetEnvironmentVariable("WORKBENCH_ENVIRONMENT") ?? "dev";

        public string TitleQualification => WorkbenchEnvironment == "prod" ? "" : $" ({WorkbenchEnvironment})";

        public IndexModel()
        {
            ParsingApiUrl = "api/single";
        }

        public void OnGet()
        {
        }
    }
}