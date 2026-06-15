using System.Text.Json.Nodes;
using Equinor.OsduCsharpClient.Facade;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Writes bulk curve data to a WellLog (<c>POST /data</c>).
/// </summary>
/// <remarks>
/// Wellbore DDMS accepts bulk data as either <c>application/json</c> or
/// <c>application/x-parquet</c>. The generated client's typed <c>Data.PostAsync</c>
/// uses the JSON path (an <c>UntypedNode</c> body sent as <c>application/json</c>),
/// which this sample demonstrates. For large datasets the Parquet path is more
/// efficient, but it transfers binary content and is not exposed through the
/// typed method — it would need a raw request.
///
/// The JSON body uses the pandas "split" orientation:
/// <c>{ "columns": [...], "index": [...], "data": [[row], …] }</c>. The first
/// column is the reference/index curve; every column name must match a curve
/// mnemonic declared in the WellLog's metadata.
/// </remarks>
public sealed class WriteBulkDataSample : ISample
{
    public string Name => "write-bulk-data";
    public string Description => "Write bulk curve data to a WellLog as JSON (/data).";
    public bool RequiresWrite => true;

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var id = ctx.ResolveWellLogId();
        SampleContext.Header($"Write bulk data (JSON) — {id}");

        // split orient: MD is the reference/index curve, GR a measurement curve.
        var payload = new JsonObject
        {
            ["columns"] = new JsonArray("MD", "GR"),
            ["index"] = new JsonArray(0, 1, 2, 3),
            ["data"] = new JsonArray(
                new JsonArray(1000.0, 75.1),
                new JsonArray(1000.5, 80.4),
                new JsonArray(1001.0, 79.9),
                new JsonArray(1001.5, 82.3)),
        };

        await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].Data
            .PostAsync(payload.ToUntypedNode(), cancellationToken: ct);

        var columns = (JsonArray)payload["columns"]!;
        var rows = (JsonArray)payload["data"]!;
        Console.WriteLine($"  Wrote {rows.Count} row(s) for columns: {string.Join(", ", columns.Select(c => (string?)c))}");
    }
}
