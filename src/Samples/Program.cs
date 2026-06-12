using Osdu.Samples;

// ── CLI ────────────────────────────────────────────────────────────────────────
//   osdu-samples                 run the read-only samples
//   osdu-samples list            list every available sample
//   osdu-samples <name> [<name>] run specific samples by name
//   flags: --write   enable opt-in write samples (also Demo:AllowWrites)
//          --verbose Debug-level SDK logging
// ---------------------------------------------------------------------------------

var argList = args.ToList();
var verbose = argList.Remove("--verbose") | argList.Remove("-v");
var writeFlag = argList.Remove("--write") | argList.Remove("-w");

if (argList is ["list"])
{
    Console.WriteLine("Available samples:\n");
    foreach (var sample in SampleRegistry.All)
    {
        var tag = sample.RequiresWrite ? " [write]" : "";
        Console.WriteLine($"  {sample.Name,-26}{sample.Description}{tag}");
    }
    Console.WriteLine("\nRun:  osdu-samples <name> [<name> …]   (default: all read-only samples)");
    return 0;
}

using var host = new SampleHost(verbose);
var writesEnabled = writeFlag || host.Demo.AllowWrites;
var context = new SampleContext(host.Client, host.Demo, writesEnabled);

// Resolve which samples to run.
List<ISample> toRun;
if (argList.Count > 0)
{
    toRun = [];
    foreach (var name in argList)
    {
        var sample = SampleRegistry.Find(name);
        if (sample is null)
        {
            Console.Error.WriteLine($"Unknown sample '{name}'. Try 'osdu-samples list'.");
            return 2;
        }
        toRun.Add(sample);
    }
}
else
{
    // Default run: every read-only sample.
    toRun = SampleRegistry.All.Where(s => !s.RequiresWrite).ToList();
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var failures = 0;
foreach (var sample in toRun)
{
    if (sample.RequiresWrite && !writesEnabled)
    {
        SampleContext.Header($"{sample.Name} — SKIPPED");
        Console.WriteLine("  Write sample. Enable with --write or Demo:AllowWrites=true.");
        continue;
    }

    try
    {
        await sample.RunAsync(context, cts.Token);
    }
    catch (OperationCanceledException) when (cts.IsCancellationRequested)
    {
        Console.Error.WriteLine("\nCancelled.");
        return 130;
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine($"  ✗ {sample.Name} failed: {ex.Message}");
    }
}

return failures == 0 ? 0 : 1;
