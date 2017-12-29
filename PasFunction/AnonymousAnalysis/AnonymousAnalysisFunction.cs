using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PasLib;
using System;

namespace PasFunction.AnonymousAnalysis
{
    public static class AnonymousAnalysisFunction
    {
        [FunctionName("AnonymousAnalysisFunction")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            TraceWriter log)
        {
            try
            {
                var body =
                    JsonSerializerHelper.Deserialize<AnonymousAnalysisInputModel>(req.Body);

                try
                {

                    if (string.IsNullOrWhiteSpace(body.Grammar))
                    {
                        return new BadRequestObjectResult("JSON body should contain 'grammar'");
                    }
                    if (string.IsNullOrWhiteSpace(body.Text))
                    {
                        return new BadRequestObjectResult("JSON body should contain 'text'");
                    }

                    var grammar = MetaGrammar.ParseGrammar(body.Grammar);

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
                    var output = JsonSerializerHelper.Serialize(outputModel);

                    return new ContentResult
                    {
                        StatusCode = 200,
                        ContentType = "application/json",
                        Content = output
                    };
                }
                catch (ParsingException ex)
                {
                    return new BadRequestObjectResult(ex.Message);
                }
                catch (Exception ex)
                {
                    log.Error($"Internal Error:  {ex.GetType().Name}, '{ex.Message}'");
                    log.Info($"Grammar:  '{body.Grammar}'");
                    log.Info($"Text:  '{body.Text}'");
                    log.Info($"Rule:  '{body.Rule}'");

                    return new ContentResult
                    {
                        StatusCode = 500,
                        ContentType = "text/plain",
                        Content = "Internal error:  please report"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error($"Body parsing error:  {ex.GetType().Name}, '{ex.Message}'");

                return new BadRequestObjectResult("JSON body isn't conform");
            }
        }
    }
}