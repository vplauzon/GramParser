using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PasFunction
{
    internal static class JsonSerializerHelper
    {
        public static string Serialize(object obj)
        {
            using (var textWriter = new StringWriter())
            {
                CreateSerializer().Serialize(textWriter, obj);

                return textWriter.ToString();
            }
        }

        public static T Deserialize<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var obj = CreateSerializer().Deserialize<T>(jsonReader);

                return obj;
            }
        }

        public static JsonSerializer CreateSerializer()
        {
            var serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            return serializer;
        }
    }
}