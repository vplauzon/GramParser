using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasLib;
using PasWebApi.Models.AnonymousAnalysis;

namespace PasWebApi.Controllers
{
    [Route("v1")]
    [Route("vnext")]
    public class AnonymousAnalysisController : Controller
    {
        [HttpPost]
        public ActionResult Post([FromBody]AnonymousAnalysisInputModel body)
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

                var match = grammar.Match(body.Rule, body.Text);

                if (match == null)
                {
                    return new BadRequestObjectResult("Text cannot be matched by grammar");
                }

                var outputModel = new AnonymousAnalysisOutputModel(match);

                return new ObjectResult(outputModel)
                {
                    StatusCode = 200
                };
            }
            catch (ParsingException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception)
            {
                //log.Error($"Internal Error:  {ex.GetType().Name}, '{ex.Message}'");
                //log.Info($"Grammar:  '{body.Grammar}'");
                //log.Info($"Text:  '{body.Text}'");
                //log.Info($"Rule:  '{body.Rule}'");

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