namespace Osdu.Samples.WellLogs;

/// <summary>Shows the Wellbore DDMS service is reachable and which version it runs.</summary>
public sealed class ServiceInfoSample : ISample
{
    public string Name => "service-info";
    public string Description => "Print Wellbore DDMS service info (/about).";

    public async Task RunAsync(SampleContext ctx, CancellationToken ct)
    {
        SampleContext.Header("Wellbore DDMS — service info");

        var about = await ctx.Client.WellboreDdms.About.GetAsync(cancellationToken: ct);

        Console.WriteLine($"  Service : {about?.Service?.String}");
        Console.WriteLine($"  Version : {about?.Version?.String}");
        Console.WriteLine($"  Release : {about?.Release?.String}");
    }
}
