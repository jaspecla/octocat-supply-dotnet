using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OctocatSupply.Api;
using OctocatSupply.Api.Data;
using OctocatSupply.Observability;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace OctocatSupply.Api.UnitTests;

[TestClass]
[DoNotParallelize]
public class TelemetryTests
{
    private const string StatsbeatSetting = "APPLICATIONINSIGHTS_STATSBEAT_DISABLED_ALL";
    private const string SdkStatsSetting = "APPLICATIONINSIGHTS_SDKSTATS_DISABLED";
    private static readonly ConcurrentQueue<string> ReceivedTelemetry = new();
    private static WebApplication collector = null!;
    private static string testConnectionString = null!;
    private static string? previousStatsbeat;
    private static string? previousSdkStats;

    [ClassInitialize]
    public static async Task StartCollector(TestContext _)
    {
        previousStatsbeat = Environment.GetEnvironmentVariable(StatsbeatSetting);
        previousSdkStats = Environment.GetEnvironmentVariable(SdkStatsSetting);
        Environment.SetEnvironmentVariable(StatsbeatSetting, "true");
        Environment.SetEnvironmentVariable(SdkStatsSetting, "true");

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        collector = builder.Build();
        collector.MapPost("/{**path}", async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            ReceivedTelemetry.Enqueue(body);
            var items = body.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                using var document = JsonDocument.Parse(item);
            }
            var count = items.Length;
            return Results.Json(new { itemsReceived = count, itemsAccepted = count, errors = Array.Empty<object>() });
        });
        await collector.StartAsync();

        var address = collector.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        testConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000001;" +
            $"IngestionEndpoint={address}/;LiveEndpoint={address}/";
    }

    [ClassCleanup]
    public static async Task StopCollector()
    {
        await collector.StopAsync();
        await collector.DisposeAsync();
        Environment.SetEnvironmentVariable(StatsbeatSetting, previousStatsbeat);
        Environment.SetEnvironmentVariable(SdkStatsSetting, previousSdkStats);
    }

    [TestMethod]
    public void NoConnectionString_LeavesTelemetryDisabled()
    {
        var builder = CreateBuilder(null);
        builder.AddApiTelemetry();

        using var host = builder.Build();
        Assert.IsNull(host.Services.GetService<TracerProvider>());
        Assert.IsNull(host.Services.GetService<MeterProvider>());
        Assert.IsFalse(host.Services.GetServices<ILoggerProvider>()
            .Any(provider => provider is OpenTelemetryLoggerProvider));
    }

    [TestMethod]
    [DataRow("octocat-supply-api")]
    [DataRow("octocat-supply-web")]
    public void ConfiguredTelemetry_UsesDistinctServiceNames(string serviceName)
    {
        const string appNameSetting = "CONTAINER_APP_NAME";
        const string replicaSetting = "CONTAINER_APP_REPLICA_NAME";
        var previousAppName = Environment.GetEnvironmentVariable(appNameSetting);
        var previousReplica = Environment.GetEnvironmentVariable(replicaSetting);
        Environment.SetEnvironmentVariable(appNameSetting, "shared-container-app");
        Environment.SetEnvironmentVariable(replicaSetting, "demo-replica");

        try
        {
            var builder = CreateBuilder(testConnectionString);
            builder.AddApplicationTelemetry(serviceName);

            using var host = builder.Build();
            var tracer = host.Services.GetRequiredService<TracerProvider>();
            var meter = host.Services.GetRequiredService<MeterProvider>();

            Assert.AreEqual(serviceName, GetServiceName(tracer));
            Assert.AreEqual(serviceName, GetServiceName(meter));
            Assert.AreEqual("demo-replica", tracer.GetResource().Attributes
                .Single(attribute => attribute.Key == "service.instance.id").Value);
            Assert.IsTrue(host.Services.GetServices<ILoggerProvider>()
                .Any(provider => provider is OpenTelemetryLoggerProvider));
        }
        finally
        {
            Environment.SetEnvironmentVariable(appNameSetting, previousAppName);
            Environment.SetEnvironmentVariable(replicaSetting, previousReplica);
        }
    }

    [TestMethod]
    public void ConfiguredServiceName_CanBeOverridden()
    {
        var builder = CreateBuilder(testConnectionString);
        builder.Configuration["OTEL_SERVICE_NAME"] = "custom-service";
        builder.AddApplicationTelemetry("default-service");

        using var host = builder.Build();
        Assert.AreEqual("custom-service", GetServiceName(host.Services.GetRequiredService<TracerProvider>()));
    }

    [TestMethod]
    public void AuthenticationWithoutConnectionString_IsRejected()
    {
        var builder = CreateBuilder(null);
        builder.Configuration["AzureMonitor:Authentication"] = "ManagedIdentity";

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.AddApplicationTelemetry("test"));
    }

    [TestMethod]
    public void NoneAuthenticationWithoutConnectionString_LeavesTelemetryDisabled()
    {
        var builder = CreateBuilder(null);
        builder.Configuration["AzureMonitor:Authentication"] = "None";
        builder.AddApplicationTelemetry("test");

        using var host = builder.Build();
        Assert.IsNull(host.Services.GetService<TracerProvider>());
    }

    [TestMethod]
    public void ManagedIdentityAuthentication_IsConfigured()
    {
        var builder = CreateBuilder(testConnectionString);
        builder.Configuration["AzureMonitor:Authentication"] = "ManagedIdentity";
        builder.Configuration["AZURE_CLIENT_ID"] = "00000000-0000-0000-0000-000000000002";
        builder.AddApplicationTelemetry("test");

        using var host = builder.Build();
        Assert.IsInstanceOfType<ManagedIdentityCredential>(
            host.Services.GetRequiredService<IOptions<AzureMonitorOptions>>().Value.Credential);
    }

    [TestMethod]
    public void SamplerConfiguration_IsApplied()
    {
        const string samplerSetting = "OTEL_TRACES_SAMPLER";
        const string samplerArgumentSetting = "OTEL_TRACES_SAMPLER_ARG";
        var previousSampler = Environment.GetEnvironmentVariable(samplerSetting);
        var previousArgument = Environment.GetEnvironmentVariable(samplerArgumentSetting);
        Environment.SetEnvironmentVariable(samplerSetting, "microsoft.fixed_percentage");
        Environment.SetEnvironmentVariable(samplerArgumentSetting, "0.25");

        try
        {
            var builder = CreateBuilder(testConnectionString);
            builder.AddApplicationTelemetry("test");

            using var host = builder.Build();
            var options = host.Services.GetRequiredService<IOptions<AzureMonitorOptions>>().Value;
            Assert.AreEqual(0.25f, options.SamplingRatio);
            Assert.IsNull(options.TracesPerSecond);
        }
        finally
        {
            Environment.SetEnvironmentVariable(samplerSetting, previousSampler);
            Environment.SetEnvironmentVariable(samplerArgumentSetting, previousArgument);
        }
    }

    [TestMethod]
    [DataRow(" ")]
    [DataRow("invalid")]
    public void InvalidAuthentication_IsRejected(string authentication)
    {
        var builder = CreateBuilder(testConnectionString);
        builder.Configuration["AzureMonitor:Authentication"] = authentication;

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.AddApplicationTelemetry("test"));
    }

    [TestMethod]
    public void WhitespaceConnectionString_IsRejected()
    {
        var builder = CreateBuilder(" ");

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.AddApplicationTelemetry("test"));
    }

    [TestMethod]
    public async Task ConfiguredProviders_CollectTracesMetricsAndCorrelatedLogs()
    {
        var builder = CreateBuilder(testConnectionString);
        var activities = new List<Activity>();
        var metrics = new List<Metric>();
        var logs = new List<LogRecord>();
        builder.AddApplicationTelemetry("octocat-supply-web");
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.SetSampler(new AlwaysOnSampler())
                .AddSource("OctocatSupply.Tests").AddInMemoryExporter(activities))
            .WithMetrics(meter => meter.AddMeter("OctocatSupply.Tests").AddInMemoryExporter(metrics))
            .WithLogging(logging => logging.AddInMemoryExporter(logs));

        using (var host = builder.Build())
        {
            await host.StartAsync();

            using var source = new ActivitySource("OctocatSupply.Tests");
            using var testMeter = new Meter("OctocatSupply.Tests");
            var counter = testMeter.CreateCounter<long>("octocat.test.requests");
            Assert.IsTrue(counter.Enabled);
            using (var activity = source.StartActivity("test-request"))
            {
                Assert.IsNotNull(activity);
                host.Services.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("OctocatSupply.Tests")
                    .LogInformation("Telemetry test message");
                counter.Add(1);

                var log = logs.Single(record => record.CategoryName == "OctocatSupply.Tests");
                Assert.AreEqual(activity.TraceId, log.TraceId);
                Assert.AreEqual(activity.SpanId, log.SpanId);
            }

            Assert.IsTrue(host.Services.GetRequiredService<TracerProvider>().ForceFlush());
            Assert.IsTrue(host.Services.GetRequiredService<MeterProvider>().ForceFlush(),
                $"Azure Monitor metric export failed; the collector received {ReceivedTelemetry.Count} batches.");
            Assert.IsTrue(activities.Any(activity => activity.DisplayName == "test-request"));
            Assert.IsTrue(metrics.Any(metric => metric.Name == "octocat.test.requests"));

            await host.StopAsync();
        }

        var exported = GetExportedItems().ToArray();
        Assert.IsTrue(exported.Any(item => GetBaseType(item) == "RemoteDependencyData" &&
            GetBaseData(item).GetProperty("name").GetString() == "test-request"));
        Assert.IsTrue(exported.Any(item => GetBaseType(item) == "MetricData" &&
            GetBaseData(item).GetProperty("metrics").EnumerateArray()
                .Any(metric => metric.GetProperty("name").GetString() == "octocat.test.requests")));
        Assert.IsTrue(exported.Any(item => GetBaseType(item) == "MessageData" &&
            GetBaseData(item).GetProperty("message").GetString() == "Telemetry test message"));
    }

    [TestMethod]
    public async Task ApiTelemetry_CollectsSqliteSpansWithoutQueryText()
    {
        const string semconvSetting = "OTEL_SEMCONV_STABILITY_OPT_IN";
        const string parameterSetting = "OTEL_DOTNET_EXPERIMENTAL_EFCORE_ENABLE_TRACE_DB_QUERY_PARAMETERS";
        var previousSemconv = Environment.GetEnvironmentVariable(semconvSetting);
        var previousParameters = Environment.GetEnvironmentVariable(parameterSetting);
        Environment.SetEnvironmentVariable(semconvSetting, "database/dup");
        Environment.SetEnvironmentVariable(parameterSetting, "true");

        try
        {
            var builder = CreateBuilder(testConnectionString);
            var activities = new List<Activity>();
            builder.AddApiTelemetry();
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.SetSampler(new AlwaysOnSampler()).AddInMemoryExporter(activities));

            using var host = builder.Build();
            await host.StartAsync();
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
            await using var context = new AppDbContext(options);
            await context.Database.ExecuteSqlRawAsync("SELECT 'otel-sensitive-marker'");
            var parameter = "otel-sensitive-marker";
            await context.Database.ExecuteSqlInterpolatedAsync($"SELECT {parameter}");
            await Assert.ThrowsExactlyAsync<SqliteException>(() =>
                context.Database.ExecuteSqlRawAsync("SELECT 'otel-sensitive-marker' FROM missing_otel_table"));

            var sqliteSpans = activities.Where(activity =>
                activity.GetTagItem("db.system")?.ToString() == "sqlite" ||
                activity.GetTagItem("db.system.name")?.ToString() == "sqlite").ToArray();
            Assert.HasCount(3, sqliteSpans);
            Assert.IsTrue(sqliteSpans.Any(activity => activity.Status == ActivityStatusCode.Error));
            foreach (var sqlite in sqliteSpans)
            {
                Assert.AreEqual("SQLite", sqlite.DisplayName);
                Assert.IsNull(sqlite.GetTagItem("db.statement"));
                Assert.IsNull(sqlite.GetTagItem("db.query.text"));
                Assert.IsNull(sqlite.GetTagItem("db.query.summary"));
                Assert.IsFalse(sqlite.TagObjects.Any(tag =>
                    tag.Key.StartsWith("db.query.parameter.", StringComparison.Ordinal) ||
                    tag.Value?.ToString()?.Contains("otel-sensitive-marker", StringComparison.Ordinal) == true));
                Assert.IsFalse(sqlite.Events.SelectMany(spanEvent => spanEvent.Tags)
                    .Any(tag => tag.Value?.ToString()?.Contains("otel-sensitive-marker", StringComparison.Ordinal) == true));
            }

            await host.StopAsync();
        }
        finally
        {
            Environment.SetEnvironmentVariable(semconvSetting, previousSemconv);
            Environment.SetEnvironmentVariable(parameterSetting, previousParameters);
        }
    }

    private static HostApplicationBuilder CreateBuilder(string? connectionString)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = connectionString,
            ["AzureMonitor:Authentication"] = null,
            ["AzureMonitor:EnableLiveMetrics"] = "false",
            ["AzureMonitor:DisableOfflineStorage"] = "true",
            ["OTEL_SERVICE_NAME"] = null,
            ["AZURE_CLIENT_ID"] = null
        });
        return builder;
    }

    private static object? GetServiceName(BaseProvider provider) =>
        provider.GetResource().Attributes.Single(attribute => attribute.Key == "service.name").Value;

    private static IEnumerable<JsonElement> GetExportedItems()
    {
        foreach (var payload in ReceivedTelemetry)
        {
            foreach (var item in payload.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                using var document = JsonDocument.Parse(item);
                yield return document.RootElement.Clone();
            }
        }
    }

    private static string? GetBaseType(JsonElement item) =>
        item.GetProperty("data").GetProperty("baseType").GetString();

    private static JsonElement GetBaseData(JsonElement item) =>
        item.GetProperty("data").GetProperty("baseData");
}
