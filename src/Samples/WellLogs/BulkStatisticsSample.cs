namespace Osdu.Samples.WellLogs;

/// <summary>Reads per-curve bulk-data statistics for a WellLog (<c>/data/statistics</c>).</summary>
public sealed class BulkStatisticsSample : ISample
{
    public string Name => "bulk-statistics";
    public string Description => "Get per-curve bulk-data statistics for a WellLog.";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var id = ctx.Require(ctx.Demo.WellLogId, nameof(ctx.Demo.WellLogId));
        SampleContext.Header($"Bulk statistics — {id}");

        var stats = await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].Data.Statistics
            .GetAsync(cancellationToken: ct);

        if (stats is null) { Console.WriteLine("  (no statistics)"); return; }

        Console.WriteLine($"  Status  : {stats.ComputationStatus}");
        Console.WriteLine($"  Record  : {stats.RecordId} (v{stats.RecordVersion})");

        var curves = stats.Data?.AdditionalData;
        if (curves is { Count: > 0 })
        {
            Console.WriteLine($"  Curves with statistics: {string.Join(", ", curves.Keys)}");
        }
    }
}
