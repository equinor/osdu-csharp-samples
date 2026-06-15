using Osdu.Samples;

// ── CLI ────────────────────────────────────────────────────────────────────────
//   osdu-samples                 run the read-only samples
//   osdu-samples list            list every available sample
//   osdu-samples <name> [<name>] run specific samples by name
//   flags: --write     enable opt-in write samples (also Demo:AllowWrites)
//          --id <id>   operate on this WellLog id (overrides Demo:WellLogId)
//          --verbose   Debug-level SDK logging
// ---------------------------------------------------------------------------------

var argList = args.ToList();
var verbose = argList.Remove("--verbose") | argList.Remove("-v");
var writeFlag = argList.Remove("--write") | argList.Remove("-w");
var wellLogId = TakeOption(argList, "--id", "-i");

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
var context = new SampleContext(host.Client, host.Demo, writesEnabled) { ActiveWellLogId = wellLogId };

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

// Removes "--id <value>" (or "-i <value>") from the argument list and returns the
// value, so it isn't treated as a sample name. Returns null when not supplied.
static string? TakeOption(List<string> args, params string[] names)
{
    for (var i = 0; i < args.Count; i++)
    {
        if (!names.Contains(args[i])) continue;
        if (i + 1 >= args.Count)
            throw new ArgumentException($"Option '{args[i]}' requires a value.");
        var value = args[i + 1];
        args.RemoveRange(i, 2);
        return value;
    }
    return null;
}
