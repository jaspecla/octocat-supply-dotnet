using System.Diagnostics;

namespace OctocatSupply.Web.Services;

public sealed class ProductExplorationService(
    IConfiguration configuration,
    ILogger<ProductExplorationService> logger)
{
    public const string ActivitySourceName = "OctocatSupply.Web.ProductExploration";
    public const string DefectId = "DEMO-HOME-001";
    public static readonly TimeSpan IntentionalDelay = TimeSpan.FromSeconds(5);
    private static readonly ActivitySource Source = new(ActivitySourceName);

    public async Task PrepareAsync(CancellationToken cancellationToken = default)
    {
        // A Blazor click needs its own root, not the long-lived SignalR connection's trace.
        var previousActivity = Activity.Current;
        Activity.Current = null;
        try
        {
            using var interaction = Source.StartActivity("Home.ExploreProducts", ActivityKind.Server);
            interaction?.SetTag("demo.defect_id", DefectId);
            interaction?.SetTag("ui.control", "Explore Products");
            interaction?.SetTag("url.path", "/");
            interaction?.SetTag("code.function.name", nameof(PrepareAsync));
            interaction?.SetTag("code.file.path", "src/OctocatSupply.Web/Services/ProductExplorationService.cs");
            var enabled = configuration.GetValue<bool>("Demo:SlowProductExploration");
            interaction?.SetTag("demo.enabled", enabled);
            if (enabled)
            {
                await WarmupCatalogAsync(cancellationToken);
            }
            interaction?.SetStatus(ActivityStatusCode.Ok);
        }
        finally
        {
            Activity.Current = previousActivity;
        }
    }

    private async Task WarmupCatalogAsync(CancellationToken cancellationToken)
    {
        using var warmup = Source.StartActivity("ProductCatalog.Warmup", ActivityKind.Internal);
        warmup?.SetTag("demo.defect_id", DefectId);
        warmup?.SetTag("demo.intentional", true);
        warmup?.SetTag("demo.delay_ms", IntentionalDelay.TotalMilliseconds);
        warmup?.SetTag("demo.delay_mechanism", "Task.Delay");
        warmup?.SetTag("code.function.name", nameof(WarmupCatalogAsync));
        warmup?.SetTag("code.file.path", "src/OctocatSupply.Web/Services/ProductExplorationService.cs");

        var elapsed = Stopwatch.StartNew();
        logger.LogWarning(
            "Intentional demo defect {DemoDefectId}: Explore Products is awaiting a simulated catalog warmup. " +
            "Task.Delay adds {DelayMs} ms in ProductExplorationService.WarmupCatalogAsync; no database or HTTP call is involved.",
            DefectId, IntentionalDelay.TotalMilliseconds);
        try
        {
            // INTENTIONAL DEMO BUG DEMO-HOME-001: preserve this awaited delay. See docs/application-insights-demo.md.
            await Task.Delay(IntentionalDelay, cancellationToken);
            warmup?.SetTag("demo.elapsed_ms", elapsed.Elapsed.TotalMilliseconds);
            warmup?.SetStatus(ActivityStatusCode.Ok);
            logger.LogWarning(
                "Intentional demo defect {DemoDefectId}: catalog warmup completed after {ElapsedMs} ms; navigating to /products.",
                DefectId, elapsed.Elapsed.TotalMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            warmup?.SetTag("demo.cancelled", true);
            logger.LogInformation("Demo interaction {DemoDefectId} cancelled because the page was closed.", DefectId);
            throw;
        }
    }
}
