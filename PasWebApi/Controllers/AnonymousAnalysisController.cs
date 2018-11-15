using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using PasLib;
using PasWebApi.Models.AnonymousAnalysis;

namespace PasWebApi.Controllers
{
    [Route("v1")]
    public class AnonymousAnalysisController : Controller
    {
        [Route("")]
        [Route("single")]
        [HttpPost]
        public ActionResult SinglePost([FromBody]AnonymousAnalysisInputModel body)
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

                var grammar = GrammarCache.ParseGrammar(body.Grammar);

                if (grammar == null)
                {
                    return new BadRequestObjectResult("Grammar cannot be parsed");
                }
                else
                {
                    var match = grammar.Match(body.Rule, body.Text);
                    var outputModel = new AnonymousAnalysisOutputModel(match);

                    TrackParsingEvent();

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

        private static void TrackParsingEvent()
        {
            //  Follow https://docs.microsoft.com/en-ca/azure/application-insights/app-insights-api-custom-events-metrics#trackevent
            var telemetry = new TelemetryClient();

            telemetry.TrackEvent("Parsing");
        }
    }
}