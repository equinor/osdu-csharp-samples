using Equinor.OsduCsharpClient.Facade;
using WellLogV15 = Osdu.Schemas.WorkProductComponent.WellLog.V1_5_0;
using WellboreV15 = Osdu.Schemas.MasterData.Wellbore.V1_5_1;
using WellV14 = Osdu.Schemas.MasterData.Well.V1_4_0;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Walks the OSDU relationship chain WellLog → Wellbore → Well by following the
/// id references inside each record's typed <c>data</c> block.
/// </summary>
public sealed class NavigateRelationshipsSample : ISample
{
    public string Name => "navigate";
    public string Description => "Follow WellLog → Wellbore → Well via data references.";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var wellLogId = ctx.ResolveWellLogId();
        var ddms = ctx.Client.WellboreDdms.Ddms.V3;

        SampleContext.Header($"WellLog — {wellLogId}");
        var log = await ddms.Welllogs[wellLogId].GetAsync(cancellationToken: ct);
        var logData = log?.Data.Deserialize<WellLogV15.Data>();
        Console.WriteLine($"  Name       : {logData?.Name}");
        Console.WriteLine($"  WellboreID : {logData?.WellboreID}");

        if (string.IsNullOrWhiteSpace(logData?.WellboreID)) return;

        SampleContext.Header($"Wellbore — {logData.WellboreID}");
        var wellbore = await ddms.Wellbores[logData.WellboreID].GetAsync(cancellationToken: ct);
        var wellboreData = wellbore?.Data.Deserialize<WellboreV15.Data>();
        Console.WriteLine($"  FacilityName : {wellboreData?.FacilityName}");
        Console.WriteLine($"  WellID       : {wellboreData?.WellID}");

        if (string.IsNullOrWhiteSpace(wellboreData?.WellID)) return;

        SampleContext.Header($"Well — {wellboreData.WellID}");
        var well = await ddms.Wells[wellboreData.WellID].GetAsync(cancellationToken: ct);
        var wellData = well?.Data.Deserialize<WellV14.Data>();
        Console.WriteLine($"  FacilityName : {wellData?.FacilityName}");
    }
}
