using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;

namespace PasWebApi
{
    public class ProjectTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }
            _httpContextAccessor = httpContextAccessor;
        }

        void ITelemetryInitializer.Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;

            if (requestTelemetry != null
                && string.IsNullOrWhiteSpace(requestTelemetry.ResponseCode))
            {
                var request = _httpContextAccessor.HttpContext.Request;
                var method = request.Method;

                if ((method == HttpMethods.Post || method == HttpMethods.Put)
                    && request.Body.CanRead)
                {
                    using (var readStream = new StreamReader(request.Body))
                    {
                        var body = readStream.ReadToEnd();

                        //  Store body in telemetry
                        requestTelemetry.Properties.Add("requestBody", body);
                        //  Reset the stream so data is not lost
                        request.Body = new MemoryStream();
                        using (var writeStream = new StreamWriter(request.Body, UTF8Encoding.ASCII, body.Length, true))
                        {
                            writeStream.Write(body);
                        }
                        request.Body.Position = 0;
                    }
                }
            }
        }
    }
}