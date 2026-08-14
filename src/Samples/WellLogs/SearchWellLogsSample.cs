using Equinor.OsduCsharpClient.Facade;
using Equinor.OsduCsharpClient.Search.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using V15 = Osdu.Schemas.WorkProductComponent.WellLog.V1_5_0;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Finds WellLogs via the Search service and reads each hit's <c>data</c> block
/// as a typed schema model.
/// </summary>
/// <remarks>
/// A Search hit is a free-form field bag (that is the Search API's shape, not a
/// client limitation): the envelope fields <c>id</c> / <c>kind</c> come back as
/// plain strings, and the requested <c>data.*</c> fields as a nested object. That
/// nested <c>data</c> is a Kiota <see cref="UntypedNode"/> — the <b>same</b> bridge
/// used elsewhere (<c>get-welllog</c>, <c>navigate</c>) deserializes it into the
/// strongly-typed <see cref="V15.Data"/> schema POCO, so domain values are read
/// through typed properties (<c>data.Name</c>, <c>data.WellboreID</c>,
/// <c>data.Curves</c>) rather than stringly-typed dictionary lookups.
/// </remarks>
public sealed class SearchWellLogsSample : ISample
{
    public string Name => "search-welllogs";
    public string Description => "Search for WellLog records and read hits as typed schema models.";

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
                // Ask Search to return the envelope id/kind plus the WellLog data
                // fields we want to read back as a typed model.
                ReturnedFields = ["id", "kind", "data.Name", "data.WellboreID", "data.Curves"],
            },
            cancellationToken: ct);

        var hits = result?.Results ?? [];
        Console.WriteLine($"  Found {hits.Count} WellLog(s):");
        foreach (var hit in hits)
        {
            var id = hit.AdditionalData.TryGetValue("id", out var idValue) ? idValue as string : null;

            // The free-form `data` object bridges to the typed WellLog schema POCO —
            // exactly the same UntypedNode.Deserialize<T>() used by get-welllog.
            var data = hit.AdditionalData.TryGetValue("data", out var dataValue) && dataValue is UntypedNode node
                ? node.Deserialize<V15.Data>()
                : null;

            Console.WriteLine($"    {id}");
            if (data is not null)
            {
                Console.WriteLine($"      Name       : {data.Name}");
                Console.WriteLine($"      WellboreID : {data.WellboreID}");
                Console.WriteLine($"      Curves     : {data.Curves?.Count ?? 0}");
            }
        }

        if (hits.Count > 0 && string.IsNullOrWhiteSpace(ctx.Demo.WellLogId))
        {
            Console.WriteLine(
                "\n  Tip: set Demo:WellLogId to one of the ids above to run the " +
                "get-welllog / navigate samples.");
        }
    }
}
