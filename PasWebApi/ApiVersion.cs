using System;
using System.Collections.Generic;
using System.Text;

namespace PasWebApi
{
    internal static class ApiVersion
    {
        public static string Number { get { return "API_VERSION_VALUE"; } }

        public static string Build { get { return "BUILD_NUMBER_VALUE"; } }

        public static string FullVersion { get { return $"FULL_VERSION_VALUE"; } }
    }
}