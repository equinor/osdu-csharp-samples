using Equinor.OsduCsharpClient.Facade;

namespace Osdu.Samples;

/// <summary>
/// Everything a sample needs at run time: the configured OSDU client, demo
/// settings, and whether write operations are permitted for this run.
/// </summary>
public sealed class SampleContext(OsduClient client, DemoOptions demo, bool writesEnabled)
{
    public OsduClient Client { get; } = client;
    public DemoOptions Demo { get; } = demo;

    /// <summary>True if write samples may mutate data this run.</summary>
    public bool WritesEnabled { get; } = writesEnabled;

    /// <summary>
    /// A WellLog id supplied for this run via the <c>--id</c> CLI flag. When set,
    /// read samples use it in place of <c>Demo:WellLogId</c> — handy for pasting the
    /// id printed by <c>ingest-welllog</c> / <c>create-welllog</c> straight into a
    /// read-back command, with no config edit.
    /// </summary>
    public string? ActiveWellLogId { get; set; }

    /// <summary>
    /// Resolves the WellLog id to operate on: the one passed via <c>--id</c> this run
    /// if present, otherwise the configured <c>Demo:WellLogId</c>.
    /// </summary>
    public string ResolveWellLogId() =>
        string.IsNullOrWhiteSpace(ActiveWellLogId)
            ? Require(Demo.WellLogId, nameof(Demo.WellLogId))
            : ActiveWellLogId;

    /// <summary>Writes a section header to the console.</summary>
    public static void Header(string title)
    {
        var rule = new string('─', 64);
        Console.WriteLine($"\n{rule}\n  {title}\n{rule}");
    }

    /// <summary>Reads a required demo value or throws a clear, actionable error.</summary>
    public string Require(string? value, string settingName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException(
                $"This sample needs 'Demo:{settingName}'. Set it in appsettings.local.json, " +
                $"user secrets, or the Demo__{settingName} environment variable.")
            : value;
}
