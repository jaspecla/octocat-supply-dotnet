using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OctocatSupply.Web.Services;

namespace OctocatSupply.Api.UnitTests;

[TestClass]
[DoNotParallelize]
public class ProductExplorationTests
{
    [TestMethod]
    public async Task DisabledByDefault_DoesNotWait()
    {
        var service = CreateService(null);
        var work = service.PrepareAsync();
        Assert.IsTrue(work.IsCompletedSuccessfully);
        await work;
    }

    [TestMethod]
    public async Task Enabled_WaitsFiveSecondsAndEmitsCorrelatedRootAndCause()
    {
        var spans = new ConcurrentQueue<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == ProductExplorationService.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Enqueue
        };
        ActivitySource.AddActivityListener(listener);
        using var unrelatedConnection = new Activity("SignalR connection").Start();
        var elapsed = Stopwatch.StartNew();

        var work = CreateService("true").PrepareAsync();
        Assert.IsFalse(work.IsCompleted, "The intentional wait must be asynchronous.");
        await work;

        Assert.AreSame(unrelatedConnection, Activity.Current, "Restore the caller's ambient trace.");
        Assert.IsTrue(elapsed.Elapsed >= TimeSpan.FromSeconds(5), "Preserve the intentional five-second defect.");
        Assert.IsTrue(elapsed.Elapsed < TimeSpan.FromSeconds(15), "The delay must remain bounded.");
        var root = spans.Single(span => span.OperationName == "Home.ExploreProducts");
        var cause = spans.Single(span => span.OperationName == "ProductCatalog.Warmup");
        Assert.AreEqual(ActivityKind.Server, root.Kind);
        Assert.AreEqual(default(ActivitySpanId), root.ParentSpanId);
        Assert.AreNotEqual(unrelatedConnection.TraceId, root.TraceId);
        Assert.AreEqual(root.TraceId, cause.TraceId);
        Assert.AreEqual(root.SpanId, cause.ParentSpanId);
        Assert.IsTrue(cause.Duration >= TimeSpan.FromSeconds(5));
        Assert.AreEqual(ProductExplorationService.DefectId, cause.GetTagItem("demo.defect_id"));
        Assert.AreEqual((object)true, cause.GetTagItem("demo.intentional"));
        Assert.AreEqual(5000d, cause.GetTagItem("demo.delay_ms"));
        Assert.AreEqual("Task.Delay", cause.GetTagItem("demo.delay_mechanism"));
        Assert.AreEqual("WarmupCatalogAsync", cause.GetTagItem("code.function.name"));
        Assert.AreEqual(ActivityStatusCode.Ok, cause.Status);
    }

    [TestMethod]
    public async Task LeavingPage_CancelsWait()
    {
        using var cancellation = new CancellationTokenSource();
        var work = CreateService("true").PrepareAsync(cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => work);
    }

    private static ProductExplorationService CreateService(string? enabled) =>
        new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Demo:SlowProductExploration"] = enabled
        }).Build(), NullLogger<ProductExplorationService>.Instance);
}
