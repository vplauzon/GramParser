using System;
using System.Collections.Generic;

namespace PasWebApi.Models.AnonymousAnalysis
{
    public class MultipleInputModel
    {
        public string Grammar { get; set; }

        public string Rule { get; set; }

        public string[] Texts { get; set; }
    }
}