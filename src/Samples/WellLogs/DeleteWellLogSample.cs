namespace Osdu.Samples.WellLogs;

/// <summary>Deletes (logical delete) a WellLog by id — useful to clean up after create-welllog.</summary>
public sealed class DeleteWellLogSample : ISample
{
    public string Name => "delete-welllog";
    public string Description => "Delete a WellLog by id (Demo:WellLogId).";
    public bool RequiresWrite => true;

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        var id = ctx.ResolveWellLogId();
        SampleContext.Header($"Delete WellLog — {id}");

        await ctx.Client.WellboreDdms.Ddms.V3.Welllogs[id].DeleteAsync(cancellationToken: ct);
        Console.WriteLine("  Deleted.");
    }
}
