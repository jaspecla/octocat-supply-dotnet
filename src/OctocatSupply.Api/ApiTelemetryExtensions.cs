using OctocatSupply.Observability;
using OpenTelemetry.Trace;

namespace OctocatSupply.Api;

public static class ApiTelemetryExtensions
{
    public static void AddApiTelemetry(this IHostApplicationBuilder builder)
    {
        builder.AddApplicationTelemetry("octocat-supply-api", tracing =>
            tracing.AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.EnrichWithIDbCommand = (activity, _) =>
                {
                    // Raw SQL can contain user input, including in otherwise sanitized queries.
                    activity.DisplayName = "SQLite";
                    activity.SetTag("db.statement", null);
                    activity.SetTag("db.query.text", null);
                    activity.SetTag("db.query.summary", null);
                    foreach (var tag in activity.TagObjects
                        .Where(tag => tag.Key.StartsWith("db.query.parameter.", StringComparison.Ordinal))
                        .ToArray())
                    {
                        activity.SetTag(tag.Key, null);
                    }
                };
            }));
    }
}
