using System;
using System.Linq;
using System.Collections.Immutable;
using System.Reflection;
using System.IO;

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

        protected string GetResource(string resourceName)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var typeNamespace = this.GetType().Namespace;
            var fullResourceName = $"{typeNamespace}.{resourceName}";

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException(
                        $"Can't find resource file '{resourceName}'",
                        nameof(resourceName));
                }
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd();

                    return text;
                }
            }
        }
    }
}