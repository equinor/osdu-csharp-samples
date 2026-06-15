using Equinor.OsduCsharpClient.Facade;
using V15 = Osdu.Schemas.WorkProductComponent.WellLog.V1_5_0;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Fetches a WellLog by id and reads its free-form <c>data</c> block as a typed
/// model via the Equinor.Osdu.Schemas POCO + the client's JSON bridge.
/// </summary>
public sealed class GetWellLogSample : ISample
{
    public string Name => "get-welllog";
    public string Description => "Get a WellLog by id and read its data with typed schema models.";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var id = ctx.ResolveWellLogId();
        SampleContext.Header($"Get WellLog — {id}");

        var record = await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].GetAsync(cancellationToken: ct);
        if (record is null) { Console.WriteLine("  (not found)"); return; }

        Console.WriteLine($"  Kind : {record.Kind}");

        // The envelope is strongly typed; `data` is a free-form UntypedNode that
        // the schemas package lets us deserialize into a typed WellLog.
        var data = record.Data.Deserialize<V15.Data>();
        if (data is not null)
        {
            Console.WriteLine($"  Name            : {data.Name}");
            Console.WriteLine($"  WellboreID      : {data.WellboreID}");
            Console.WriteLine($"  Top / Bottom MD : {data.TopMeasuredDepth} / {data.BottomMeasuredDepth}");
            Console.WriteLine($"  Curves          : {data.Curves?.Count ?? 0}");
            foreach (var curve in data.Curves ?? [])
                Console.WriteLine($"      - {curve.Mnemonic}  ({curve.CurveDescription})");
        }
    }
}
