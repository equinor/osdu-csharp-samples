namespace Osdu.Samples;

/// <summary>
/// A single, self-contained demonstration of one OSDU operation.
/// Implementations are discovered automatically by <see cref="SampleRegistry"/>.
/// </summary>
public interface ISample
{
    /// <summary>Short kebab-case name used to select the sample on the command line.</summary>
    string Name { get; }

    /// <summary>One-line description shown by the <c>list</c> command.</summary>
    string Description { get; }

    /// <summary>
    /// True if the sample creates, updates or deletes data. Write samples are
    /// skipped unless writes are explicitly enabled (Demo:AllowWrites or --write).
    /// </summary>
    bool RequiresWrite => false;

    Task RunAsync(SampleContext context, CancellationToken cancellationToken);
}
