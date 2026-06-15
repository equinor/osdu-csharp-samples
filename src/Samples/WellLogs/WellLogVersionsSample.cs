namespace Osdu.Samples.WellLogs;

/// <summary>Lists all stored versions of a WellLog record.</summary>
public sealed class WellLogVersionsSample : ISample
{
    public string Name => "welllog-versions";
    public string Description => "List all stored versions of a WellLog.";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var id = ctx.ResolveWellLogId();
        SampleContext.Header($"WellLog versions — {id}");

        var versions = await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].Versions
            .GetAsync(cancellationToken: ct);

        // RecordVersions.Versions is a composed type; the numeric version list
        // lives on its Int64 member.
        var numbers = versions?.Versions?.Int64 ?? [];
        Console.WriteLine($"  {numbers.Count} version(s):");
        foreach (var v in numbers)
            Console.WriteLine($"    {v}");
    }
}
