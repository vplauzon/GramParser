using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace PasFunction
{
    public static class GrammarFunction
    {
        [FunctionName("GrammarFunction")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            TraceWriter log)
        {
            log.Info("Begin request.");

            return new OkObjectResult($"Hello world");
                //: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}