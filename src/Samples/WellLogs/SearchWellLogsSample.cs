using Equinor.OsduCsharpClient.Search.Models;

namespace Osdu.Samples.WellLogs;

/// <summary>Finds WellLogs via the Search service (kind + free-text query).</summary>
public sealed class SearchWellLogsSample : ISample
{
    public string Name => "search-welllogs";
    public string Description => "Search for WellLog records by kind.";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        SampleContext.Header("Search — WellLogs");

        var result = await ctx.Client.Search.Query.PostAsync(
            new QueryRequest
            {
                Kind = new QueryRequest.QueryRequest_kind
                {
                    QueryRequestKindString = "osdu:wks:work-product-component--WellLog:*",
                },
                Query = "*",
                Limit = 10,
                ReturnedFields = ["id", "kind", "createTime"],
            },
            cancellationToken: ct);

        var hits = result?.Results ?? [];
        Console.WriteLine($"  Found {hits.Count} WellLog(s):");
        foreach (var record in hits)
        {
            record.AdditionalData.TryGetValue("id", out var id);
            record.AdditionalData.TryGetValue("kind", out var kind);
            Console.WriteLine($"    {id}   ({kind})");
        }

        if (hits.Count > 0 && string.IsNullOrWhiteSpace(ctx.Demo.WellLogId))
        {
            Console.WriteLine(
                "\n  Tip: set Demo:WellLogId to one of the ids above to run the " +
                "get-welllog / navigate samples.");
        }
    }
}
