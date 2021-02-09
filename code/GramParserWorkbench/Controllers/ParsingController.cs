using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GramParserLib;
using GramParserWorkbench.Models.Apis;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace GramParserWorkbench.Controllers
{
    [ApiController]
    [Route("api")]
    public class ParsingController : Controller
    {
        private readonly TelemetryClient _telemetryClient;

        public ParsingController(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        [Route("single")]
        [HttpPost]
        public ActionResult SinglePost([FromBody] SingleInputModel body)
        {
            try
            {
                if (body == null)
                {
                    return new BadRequestObjectResult("No JSON body");
                }
                if (string.IsNullOrWhiteSpace(body.Grammar))
                {
                    return new BadRequestObjectResult("JSON body should contain 'grammar'");
                }
                if (string.IsNullOrWhiteSpace(body.Text))
                {
                    return new BadRequestObjectResult("JSON body should contain 'text'");
                }

                var grammarWatch = Stopwatch.StartNew();
                var grammar = GrammarCache.ParseGrammar(body.Grammar);
                var grammarDuration = grammarWatch.Elapsed;

                if (grammar == null)
                {
                    return new BadRequestObjectResult("Grammar cannot be parsed");
                }
                else
                {
                    var matchWatch = Stopwatch.StartNew();
                    var match = grammar.Match(body.Rule, body.Text);
                    var matchDuration = matchWatch.Elapsed;
                    var outputModel = new SingleOutputModel(
                        match,
                        grammarDuration,
                        matchDuration);

                    _telemetryClient.TrackEvent("Parsing");

                    return new ObjectResult(outputModel)
                    {
                        StatusCode = 200
                    };
                }
            }
            catch (ParsingException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    ContentType = "text/plain",
                    Content = "Internal error:  please report"
                };
            }
        }
    }
}