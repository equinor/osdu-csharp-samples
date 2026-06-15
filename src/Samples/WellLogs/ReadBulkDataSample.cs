using Equinor.OsduCsharpClient.Facade;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Reads a WellLog's bulk curve data as JSON. Wellbore DDMS can return bulk data
/// as <c>application/json</c> or <c>application/x-parquet</c>; the generated
/// client requests JSON (<c>Accept: application/json</c>) and exposes it as a
/// Kiota <see cref="Microsoft.Kiota.Abstractions.Serialization.UntypedNode"/>,
/// which the client's JSON bridge turns into <c>System.Text.Json</c> for
/// inspection. The JSON uses the pandas "split" orientation
/// (<c>{ "columns", "index", "data" }</c>).
/// </summary>
public sealed class ReadBulkDataSample : ISample
{
    public string Name => "read-bulk-data";
    public string Description => "Read a WellLog's bulk curve data (/data).";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var id = ctx.ResolveWellLogId();
        SampleContext.Header($"Read bulk data — {id}");

        var bulk = await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].Data
            .GetAsync(cancellationToken: ct);

        var json = bulk.ToJsonNode();
        if (json is null) { Console.WriteLine("  (no bulk data)"); return; }

        // Bulk payloads can be large — print a truncated preview.
        var text = json.ToJsonString();
        Console.WriteLine($"  {text[..Math.Min(800, text.Length)]}");
        if (text.Length > 800) Console.WriteLine($"  … ({text.Length:N0} chars total)");
    }
}
