using System;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    internal static class ExtractorHelper
    {
        public static object CleanOutput(object output)
        {
            var stringLike = output as IEnumerable<char>;

            if (stringLike != null && !(output is string))
            {
                return new string(stringLike.ToArray());
            }

            return output;
        }
    }
}