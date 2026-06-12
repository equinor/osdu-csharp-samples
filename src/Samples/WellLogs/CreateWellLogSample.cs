using Equinor.OsduCsharpClient.Facade;
using Equinor.OsduCsharpClient.WellboreDdms.Models;
using V15 = Osdu.Schemas.WorkProductComponent.WellLog.V1_5_0;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Creates a WellLog from a typed schema model. Demonstrates authoring the
/// free-form <c>data</c> block with strongly-typed POCOs and the JSON bridge.
/// </summary>
public sealed class CreateWellLogSample : ISample
{
    public string Name => "create-welllog";
    public string Description => "Create a WellLog from a typed schema model.";
    public bool RequiresWrite => true;

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var wellboreId = ctx.Require(ctx.Demo.WellboreId, nameof(ctx.Demo.WellboreId));
        var legalTag = ctx.Require(ctx.Demo.LegalTag, nameof(ctx.Demo.LegalTag));
        var aclOwner = ctx.Require(ctx.Demo.AclOwner, nameof(ctx.Demo.AclOwner));
        var aclViewer = ctx.Require(ctx.Demo.AclViewer, nameof(ctx.Demo.AclViewer));

        SampleContext.Header("Create WellLog");

        var data = new V15.Data
        {
            Name = "osdu-csharp-samples GR log",
            WellboreID = wellboreId,
            TopMeasuredDepth = 1000.0,
            BottomMeasuredDepth = 2000.0,
            IsRegular = true,
            Curves = [new V15.Curves { CurveID = "GR", Mnemonic = "GR", CurveDescription = "Gamma Ray", NumberOfColumns = 1 }],
        };

        var record = new Record
        {
            Kind = "osdu:wks:work-product-component--WellLog:1.5.0",
            Acl = new StorageAcl { Owners = [aclOwner], Viewers = [aclViewer] },
            Legal = new Legal
            {
                Legaltags = new Legal.Legal_legaltags { String = [legalTag] },
                OtherRelevantDataCountries = new Legal.Legal_otherRelevantDataCountries { String = ["US"] },
            },
            Data = data.ToUntypedNode(),
        };

        var response = await ctx.Client.WellboreDdms.Ddms.V3.Welllogs
            .PostAsync([record], cancellationToken: ct);

        var ids = response?.RecordIds?.String ?? [];
        Console.WriteLine($"  Created {ids.Count} record(s):");
        foreach (var id in ids)
            Console.WriteLine($"    {id}");
        Console.WriteLine("\n  Tip: set Demo:WellLogId to a created id to run the read samples, " +
                          "then 'delete-welllog' to clean up.");
    }
}
