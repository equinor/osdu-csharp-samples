using System.Text.Json;
using Equinor.OsduCsharpClient.Facade;
using Equinor.OsduCsharpClient.WellboreDdms.Models;
using V15 = Osdu.Schemas.WorkProductComponent.WellLog.V1_5_0;

namespace Osdu.Samples.WellLogs;

/// <summary>
/// Ingests a WellLog and its bulk curve data into OSDU <b>from files</b>: a
/// typed WellLog <c>data</c> JSON file and a Parquet file of curve values.
/// </summary>
/// <remarks>
/// Showcases the client and schema libraries working together end-to-end:
/// <list type="number">
///   <item>The <c>data</c> document from <c>Demo:WellLogDataFile</c> is
///         deserialized into the strongly-typed <see cref="V15.Data"/> schema
///         model — so it is validated and editable as POCOs.</item>
///   <item>The record is assembled with the typed <see cref="Record"/>,
///         <see cref="StorageAcl"/> and <see cref="Legal"/> models (ACL/Legal come
///         from <c>Demo</c> config; the parent WellboreID is optional — taken from the
///         data file or, if absent, the optional <c>Demo:WellboreId</c>), then bridged
///         to the API via <c>Data.ToUntypedNode()</c> and posted with
///         <c>WellboreDdms.Ddms.V3.Welllogs.PostAsync</c>.</item>
///   <item>The Parquet file from <c>Demo:ParquetFile</c> is streamed to the new
///         record as <c>application/x-parquet</c> via the client's
///         <see cref="OsduClient.WellboreDdmsBulk"/> helper.</item>
/// </list>
/// Parquet columns must match the curve mnemonics declared in the <c>data</c>
/// document. File paths default to the bundled <c>sample-data</c> files when unset.
/// </remarks>
public sealed class IngestWellLogSample : ISample
{
    public string Name => "ingest-welllog";
    public string Description => "Ingest a WellLog (typed schema) and its Parquet bulk data from files.";
    public bool RequiresWrite => true;

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var legalTag = ctx.Require(ctx.Demo.LegalTag, nameof(ctx.Demo.LegalTag));
        var aclOwner = ctx.Require(ctx.Demo.AclOwner, nameof(ctx.Demo.AclOwner));
        var aclViewer = ctx.Require(ctx.Demo.AclViewer, nameof(ctx.Demo.AclViewer));

        var dataFile = ResolveFile(ctx.Demo.WellLogDataFile, "welllog-data.json");
        var parquetFile = ResolveFile(ctx.Demo.ParquetFile, "welllog-bulk.parquet");

        SampleContext.Header("Ingest WellLog from files");
        Console.WriteLine($"  Data:    {dataFile}");
        Console.WriteLine($"  Parquet: {parquetFile}");

        // Read the WellLog data block into the typed schema model.
        var json = await File.ReadAllTextAsync(dataFile, ct);
        var data = JsonSerializer.Deserialize<V15.Data>(json)
                   ?? throw new InvalidOperationException($"Could not parse WellLog data from '{dataFile}'.");

        // WellboreID is optional in the WellLog schema. Use the data file's value
        // if present, else fall back to the (optional) Demo:WellboreId config.
        if (string.IsNullOrWhiteSpace(data.WellboreID) && !string.IsNullOrWhiteSpace(ctx.Demo.WellboreId))
            data.WellboreID = ctx.Demo.WellboreId;

        Console.WriteLine($"  Wellbore: {(string.IsNullOrWhiteSpace(data.WellboreID) ? "(none — parentless)" : data.WellboreID)}");

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

        var id = response?.RecordIds?.String?.FirstOrDefault()
                 ?? throw new InvalidOperationException("No record id was returned for the created WellLog.");
        Console.WriteLine($"  Created WellLog: {id}");

        var mnemonics = (data.Curves ?? []).Select(c => c.Mnemonic).Where(m => m is not null);
        Console.WriteLine($"  Curves: {string.Join(", ", mnemonics)}");

        await using var parquet = File.OpenRead(parquetFile);
        await ctx.Client.WellboreDdmsBulk.WriteParquetAsync(id, parquet, cancellationToken: ct);

        Console.WriteLine($"  Wrote {new FileInfo(parquetFile).Length:N0} bytes of Parquet bulk data.");
        Console.WriteLine("\n  Tip: read it back by passing the id above, e.g.\n" +
                          $"       dotnet run --project src/Samples -- get-welllog read-bulk-data bulk-statistics --id {id}\n" +
                          $"       and clean up with: ... delete-welllog --id {id} --write");
    }

    /// <summary>
    /// Resolves a configured path, falling back to a bundled <c>sample-data</c>
    /// file. Relative paths resolve against the app directory.
    /// </summary>
    private static string ResolveFile(string? configured, string bundledName)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var path = Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(AppContext.BaseDirectory, configured);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Configured file not found: {path}");
            return path;
        }

        var bundled = Path.Combine(AppContext.BaseDirectory, "sample-data", bundledName);
        if (!File.Exists(bundled))
            throw new FileNotFoundException(
                $"No file configured and bundled sample '{bundled}' is missing. " +
                "Set Demo:WellLogDataFile / Demo:ParquetFile to your own files.");
        return bundled;
    }
}
