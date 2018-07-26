using System;
using System.Collections.Generic;
using System.Text;

namespace PasWebApi
{
    internal static class ApiVersion
    {
        public static string Number { get { return "0.0.97"; } }

        public static string Build { get { return "BUILD_VALUE"; } }

        public static string FullVersion { get { return $"{Number}.{Build}"; } }
    }
}