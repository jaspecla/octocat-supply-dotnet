using OctocatSupply.Observability;
using OctocatSupply.Web.Services;
using OpenTelemetry.Trace;

namespace OctocatSupply.Web;

public static class WebTelemetryExtensions
{
    public static void AddWebTelemetry(this IHostApplicationBuilder builder)
    {
        builder.AddApplicationTelemetry("octocat-supply-web", tracing =>
        {
            tracing.AddSource(ProductExplorationService.ActivitySourceName);
            if (builder.Configuration.GetValue<bool>("Demo:SlowProductExploration"))
            {
                // Keep every demo click available for diagnosis, even at low traffic.
                tracing.SetSampler(new AlwaysOnSampler());
            }
        });
    }
}
