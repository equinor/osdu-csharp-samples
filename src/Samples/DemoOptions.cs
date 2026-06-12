namespace Osdu.Samples;

/// <summary>
/// Demo-specific settings bound from the <c>Demo</c> configuration section.
/// These identify existing records to read and supply the metadata needed for
/// the opt-in write samples.
/// </summary>
public sealed record DemoOptions
{
    /// <summary>Master switch for the write samples (create / update / delete).</summary>
    public bool AllowWrites { get; init; }

    /// <summary>An existing WellLog id for the read/navigate/statistics samples.</summary>
    public string? WellLogId { get; init; }

    /// <summary>Parent Wellbore id used when creating a WellLog.</summary>
    public string? WellboreId { get; init; }

    /// <summary>A valid legal tag for created records.</summary>
    public string? LegalTag { get; init; }

    /// <summary>Owner ACL group email for created records.</summary>
    public string? AclOwner { get; init; }

    /// <summary>Viewer ACL group email for created records.</summary>
    public string? AclViewer { get; init; }

    /// <summary>
    /// Path to a WellLog <c>data</c> JSON file (a <c>WellLog:1.5.0</c> data block)
    /// for the <c>ingest-welllog</c> sample. Relative paths resolve against the app
    /// directory; defaults to the bundled <c>sample-data/welllog-data.json</c> when unset.
    /// </summary>
    public string? WellLogDataFile { get; init; }

    /// <summary>
    /// Path to a Parquet file with bulk curve data for the <c>ingest-welllog</c>
    /// sample. Relative paths resolve against the app directory; defaults to the
    /// bundled <c>sample-data/welllog-bulk.parquet</c> when unset.
    /// </summary>
    public string? ParquetFile { get; init; }
}
