using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasLib;
using PasWebApi.Models.AnonymousAnalysis;

namespace PasWebApi.Controllers
{
    /// <summary>Used for probes.</summary>
    [Route("/")]
    public class RootController : Controller
    {
        [HttpGet]
        public ActionResult Get()
        {
            return new ObjectResult(new
            {
                Version = ApiVersion.FullVersion,
                Status = "Ready"
            });
        }
    }
}