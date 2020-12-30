using System;
using System.Linq;
using System.Collections.Immutable;

namespace GramParserLibUnitTest
{
    public class BaseTest
    {
        protected IImmutableList<object> ToList(object output)
        {
            return (IImmutableList<object>)output;
        }

        protected IImmutableDictionary<string, object> ToMap(object output)
        {
            return (IImmutableDictionary<string, object>)output;
        }
    }
}