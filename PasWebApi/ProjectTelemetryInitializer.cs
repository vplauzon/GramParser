using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace PasWebApi
{
    internal class ProjectTelemetryInitializer : ITelemetryInitializer
    {
        void ITelemetryInitializer.Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = "PasWebApi";
        }
    }
}