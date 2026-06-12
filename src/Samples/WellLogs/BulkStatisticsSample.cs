using Equinor.OsduCsharpClient.WellboreDdms.Models;
using Microsoft.Kiota.Abstractions;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Reads per-curve bulk-data statistics for a WellLog (<c>GET /data/statistics</c>).
/// </summary>
/// <remarks>
/// Wellbore DDMS computes statistics out-of-band, so it's a two-step flow:
/// <list type="number">
///   <item><c>POST /ddms/v3/welllogs/{id}/versions/{version}/data/statistics</c>
///         triggers the computation.</item>
///   <item><c>GET /ddms/v3/welllogs/{id}/data/statistics</c> returns them — but
///         <b>404</b> until a computation has been triggered for the record.</item>
/// </list>
/// This sample reads the statistics and, if they have not been computed yet
/// (404 or non-<c>complete</c> status), triggers the computation on the latest
/// version and polls until it finishes.
/// </remarks>
public sealed class BulkStatisticsSample : ISample
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private const int MaxPolls = 15;

    public string Name => "bulk-statistics";
    public string Description => "Get per-curve bulk-data statistics for a WellLog.";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var id = ctx.Require(ctx.Demo.WellLogId, nameof(ctx.Demo.WellLogId));
        SampleContext.Header($"Bulk statistics — {id}");

        var stats = await TryGetStatisticsAsync(ctx, id, ct);

        if (stats?.ComputationStatus is not BulkStatisticsStatus.Complete)
        {
            stats = await ComputeStatisticsAsync(ctx, id, stats, ct);
            if (stats is null) return;
        }

        Print(stats);
    }

    /// <summary>GETs statistics, treating a 404 (never computed) as "no statistics yet".</summary>
    private static async Task<BulkDataStatisticsResponse?> TryGetStatisticsAsync(
        SampleContext ctx, string id, CancellationToken ct)
    {
        try
        {
            return await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].Data.Statistics
                .GetAsync(cancellationToken: ct);
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            return null;
        }
    }

    /// <summary>Triggers computation on the latest version and polls until it completes.</summary>
    private static async Task<BulkDataStatisticsResponse?> ComputeStatisticsAsync(
        SampleContext ctx, string id, BulkDataStatisticsResponse? current, CancellationToken ct)
    {
        var versions = await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].Versions
            .GetAsync(cancellationToken: ct);
        var latest = versions?.Versions?.Int64 is { Count: > 0 } v ? v.Max() : (long?)null;
        if (latest is null)
        {
            Console.WriteLine("  (no versions found; cannot compute statistics)");
            return null;
        }

        Console.WriteLine($"  Status: {current?.ComputationStatus?.ToString() ?? "not computed"} — " +
                          $"triggering computation for version {latest}…");

        await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].Versions[latest.Value].Data.Statistics
            .PostAsync(cancellationToken: ct);

        for (var attempt = 1; attempt <= MaxPolls; attempt++)
        {
            await Task.Delay(PollInterval, ct);
            var stats = await TryGetStatisticsAsync(ctx, id, ct);

            switch (stats?.ComputationStatus)
            {
                case BulkStatisticsStatus.Complete:
                    Console.WriteLine($"  Computation complete after {attempt * PollInterval.TotalSeconds:F0}s.");
                    return stats;
                case BulkStatisticsStatus.Error:
                    Console.WriteLine("  Computation reported an error.");
                    return stats;
            }
        }

        Console.WriteLine($"  Still computing after {MaxPolls * PollInterval.TotalSeconds:F0}s; " +
                          "re-run 'bulk-statistics' shortly to read the result.");
        return null;
    }

    private static void Print(BulkDataStatisticsResponse stats)
    {
        Console.WriteLine($"  Status  : {stats.ComputationStatus}");
        Console.WriteLine($"  Record  : {stats.RecordId} (v{stats.RecordVersion})");

        var curves = stats.Data?.AdditionalData;
        if (curves is { Count: > 0 })
            Console.WriteLine($"  Curves with statistics: {string.Join(", ", curves.Keys)}");
    }
}
