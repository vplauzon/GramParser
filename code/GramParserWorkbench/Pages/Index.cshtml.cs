using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GramParserLib;
using GramParserWorkbench.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GramParserWorkbench.Pages
{
    public class IndexModel : PageModel
    {
        private readonly TelemetryClient _telemetryClient;

        public string ParsingApiUrl { get; set; }

        public string ParserVersion => AppVersionHelper.ParserVersion;

        public string InstrumentationKey { get; set; }

        public IndexModel(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;

            ParsingApiUrl = "api/single";
            InstrumentationKey = _telemetryClient.InstrumentationKey;
        }

        public void OnGet()
        {
        }
    }
}