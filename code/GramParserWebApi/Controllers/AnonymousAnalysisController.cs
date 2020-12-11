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
    [ApiController]
    [Route("v1")]
    public class AnonymousAnalysisController : Controller
    {
        [Route("single")]
        [HttpPost]
        public ActionResult SinglePost([FromBody]SingleInputModel body)
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
                    var outputModel = new SingleOutputModel(match);

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

        [Route("")]
        [Route("multiple")]
        [HttpPost]
        public ActionResult MultiplePostAsync([FromBody]MultipleInputModel body)
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
                if (body.Texts == null || !body.Texts.Any())
                {
                    return new BadRequestObjectResult("JSON body should contain 'texts' with at least one text");
                }

                var grammar = GrammarCache.ParseGrammar(body.Grammar);

                if (grammar == null)
                {
                    return new BadRequestObjectResult("Grammar cannot be parsed");
                }
                else
                {
                    var matches = from text in body.Texts
                                  select grammar.Match(body.Rule, text);
                    var outputs = from match in matches
                                  select new SingleOutputModel(match);
                    var outputModels = outputs.ToArray();

                    TrackParsingEvent();

                    return new ObjectResult(outputModels)
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