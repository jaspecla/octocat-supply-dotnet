using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OctocatSupply.Observability;

public static class TelemetryExtensions
{
    public static void AddApplicationTelemetry(
        this IHostApplicationBuilder builder,
        string defaultServiceName,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        var authentication = builder.Configuration["AzureMonitor:Authentication"];

        if (string.IsNullOrEmpty(connectionString))
        {
            if (!string.IsNullOrEmpty(authentication) &&
                !authentication.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "APPLICATIONINSIGHTS_CONNECTION_STRING is required when Azure Monitor authentication is configured.");
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("APPLICATIONINSIGHTS_CONNECTION_STRING cannot be whitespace.");
        }

        var configuredServiceName = builder.Configuration["OTEL_SERVICE_NAME"];
        if (configuredServiceName is not null && string.IsNullOrWhiteSpace(configuredServiceName))
        {
            throw new InvalidOperationException("OTEL_SERVICE_NAME cannot be empty or whitespace.");
        }

        var credential = CreateCredential(builder.Configuration, authentication);
        var telemetry = builder.Services.AddOpenTelemetry();

        if (configureTracing is not null)
        {
            telemetry.WithTracing(configureTracing);
        }

        telemetry.UseAzureMonitor();
        builder.Services.Configure<AzureMonitorOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.Credential = credential;
            options.EnableLiveMetrics = builder.Configuration.GetValue("AzureMonitor:EnableLiveMetrics", true);
            options.DisableOfflineStorage = builder.Configuration.GetValue("AzureMonitor:DisableOfflineStorage", false);
        });
        telemetry.ConfigureResource(resource =>
            resource.AddService(configuredServiceName ?? defaultServiceName, autoGenerateServiceInstanceId: false));
    }

    private static TokenCredential? CreateCredential(IConfiguration configuration, string? authentication)
    {
        if (authentication is null || authentication.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (authentication.Equals("DefaultAzureCredential", StringComparison.OrdinalIgnoreCase))
        {
            return new DefaultAzureCredential();
        }

        if (authentication.Equals("ManagedIdentity", StringComparison.OrdinalIgnoreCase))
        {
            var clientId = configuration["AZURE_CLIENT_ID"];
            if (string.IsNullOrEmpty(clientId))
            {
                return new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException("AZURE_CLIENT_ID cannot be whitespace.");
            }

            return new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId));
        }

        throw new InvalidOperationException(
            "AzureMonitor:Authentication must be None, ManagedIdentity, or DefaultAzureCredential.");
    }
}
