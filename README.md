# OSDU C# Samples

Runnable, focused examples of using the [`Equinor.OsduCsharpClient`][client] and
[`Equinor.Osdu.Models`][models] packages against OSDU — centred on **Wellbore
DDMS well logs**. Each sample is a small, self-contained class you can read as
documentation and run on its own.

[client]: https://github.com/equinor/osdu-csharp-client
[models]: https://github.com/equinor/osdu-csharp-models

## How the two libraries fit together

The packages are complementary and designed to be used side by side:

- **`Equinor.OsduCsharpClient`** is a generated client for the OSDU APIs. It gives
  you strongly-typed *service* calls and record *envelopes* (`Record`, `StorageAcl`,
  `Legal`, search requests, …) but treats each record's domain `data` block as a
  free-form `UntypedNode`, because a single client cannot hard-code every OSDU kind.
- **`Equinor.Osdu.Models`** supplies strongly-typed POCOs for that `data` block —
  one per OSDU kind and version (e.g. `WellLog:1.5.0`, `Wellbore:1.5.1`).

They meet at a small JSON bridge exposed by the client, so you get end-to-end typing
with no hand-written DTOs or stringly-typed dictionary access:

```csharp
using Equinor.OsduCsharpClient.Facade; // Deserialize<T>() / ToUntypedNode()
using V15 = Osdu.Models.WorkProductComponent.WellLog.V1_5_0;

// Read: envelope from the client, data as a typed schema POCO.
var record = await client.WellboreDdms.Ddms.V3.Welllogs[id].GetAsync();
V15.Data data = record.Data.Deserialize<V15.Data>();   // UntypedNode → POCO
Console.WriteLine(data.Name);                           // typed property, not data["name"]

// Write: author the data as a POCO, bridge back to the envelope.
record.Data = data.ToUntypedNode();                     // POCO → UntypedNode
```

The same `Deserialize<T>()` bridge works anywhere the client hands back a `data`
block — including Search hits (see `search-welllogs`). Bulk curve values (`/data`)
are the exception: they are tabular, not a schema kind, so they stay untyped JSON or
Parquet.

## Samples

| Name | Description | Writes? |
|---|---|---|
| `service-info` | Print Wellbore DDMS service info (`/about`). | |
| `search-welllogs` | Search for WellLogs and read each hit's `data` as a typed schema model. | |
| `get-welllog` | Get a WellLog by id and read its `data` with typed schema models. | |
| `welllog-versions` | List all stored versions of a WellLog. | |
| `navigate` | Follow WellLog → Wellbore → Well via data references. | |
| `read-bulk-data` | Read a WellLog's bulk curve data as JSON (`/data`). | |
| `bulk-statistics` | Get per-curve bulk-data statistics (`/data/statistics`). | |
| `create-welllog` | Create a WellLog from a typed schema model. | ✍️ |
| `write-bulk-data` | Write bulk curve data to a WellLog as JSON (`/data`). | ✍️ |
| `ingest-welllog` | Ingest a WellLog (typed schema) and its Parquet bulk data from files. | ✍️ |
| `delete-welllog` | Delete a WellLog by id. | ✍️ |

Bulk data (`/data`) can be transferred as **JSON** (pandas "split" orientation —
`{ columns, index, data }`) or **Parquet** (`application/x-parquet`). The
`write-bulk-data` sample uses the JSON path (exposed by the generated client as a
typed `UntypedNode`); the `ingest-welllog` sample uses the binary Parquet path via
the client's `WellboreDdmsBulk` helper, which is more efficient for large data.

## Running

With the .NET 10 SDK and [NuGet access](#nuget-access) configured:

```sh
dotnet build
dotnet run --project src/Samples -- list
dotnet run --project src/Samples -- get-welllog
```

The executable accepts the same sample names and flags:

```sh
osdu-samples                 # run all read-only samples
osdu-samples list            # list every sample
osdu-samples get-welllog     # run one sample
osdu-samples search-welllogs get-welllog   # run several
```

Flags: `--write` enables the opt-in write samples (or set `Demo:AllowWrites=true`);
`--id <welllog-id>` operates on a specific WellLog id, overriding `Demo:WellLogId`;
`--verbose` turns on Debug-level SDK request/response logging.

`--id` makes the ingest → read-back demo flow config-free — paste the id printed by
`ingest-welllog` straight into the read commands:

```sh
osdu-samples ingest-welllog --write
# → Created WellLog: dev:work-product-component--WellLog:<new-id>
osdu-samples get-welllog read-bulk-data bulk-statistics --id dev:work-product-component--WellLog:<new-id>
osdu-samples delete-welllog --id dev:work-product-component--WellLog:<new-id> --write   # clean up
```

## Configuration

Uses standard .NET configuration. Provide values in `appsettings.local.json`
(gitignored), user secrets, or `Osdu__*` / `Demo__*` environment variables:

```json
{
  "Osdu": {
    "Server": "https://your-osdu-instance.com",
    "DataPartitionId": "your-partition-id",
    "Authority": "https://login.microsoftonline.com/<tenant-id>",
    "ClientId": "<client-id>",
    "Scopes": "api://<app-id-uri>/.default"
  },
  "Demo": {
    "AllowWrites": false,
    "WellLogId": "<partition>:work-product-component--WellLog:<id>:",
    "WellboreId": "<partition>:master-data--Wellbore:<id>:",
    "LegalTag": "<partition>-...-dataset-1",
    "AclOwner": "data.default.owners@<partition>.<domain>",
    "AclViewer": "data.default.viewers@<partition>.<domain>",
    "WellLogDataFile": "",
    "ParquetFile": ""
  }
}
```

These samples use the optional **`Equinor.OsduCsharpClient.Msal`** package for
interactive authentication (browser on first run, then silent renewal from cache).
The core client no longer supplies a default token provider, so `SampleHost`
passes one explicitly:

```csharp
using Equinor.OsduCsharpClient.Facade;
using Equinor.OsduCsharpClient.Msal;

var config = OsduConfig.FromConfiguration(configuration);
var tokenProvider = new MsalInteractiveTokenProvider(config, loggerFactory: loggerFactory);
using var client = new OsduClient(config, tokenProvider, loggerFactory: loggerFactory);
```

`Authority`, `ClientId`, and `Scopes` remain required for this MSAL provider.
Read samples need only the `Osdu` section plus
`Demo:WellLogId` (or `--id`); write samples additionally need `WellboreId`, `LegalTag`,
`AclOwner`, `AclViewer`.

`ingest-welllog` reads a typed WellLog `data` JSON file and a Parquet file. Point
`Demo:WellLogDataFile` and `Demo:ParquetFile` at your own files, or leave them
empty to use the bundled `sample-data/welllog-data.json` and
`sample-data/welllog-bulk.parquet`. The `data` document is deserialized into the
typed `WellLog:1.5.0` schema model; ACL, legal tag and the parent `WellboreID`
come from the `Demo` config above, so the bundled example works against any
instance. Parquet columns must match the curve mnemonics declared in the `data`.

## NuGet access

`Equinor.OsduCsharpClient`, `Equinor.OsduCsharpClient.Msal`, and
`Equinor.Osdu.Models` are published to GitHub
Packages. `nuget.config` declares the `equinor-github` source; add credentials
(a PAT with `read:packages`) to your **user-level** NuGet config:

```sh
dotnet nuget add source https://nuget.pkg.github.com/equinor/index.json \
  --name equinor-github --username <you> --password <PAT> --store-password-in-clear-text
```

## Status

This project targets **.NET 10**, the **2.x OSDU client** with its optional MSAL
package, and **`Equinor.Osdu.Models` 1.x**. Exact package versions are pinned in
[`Samples.csproj`](src/Samples/Samples.csproj). Model aliases use `Osdu.Models`,
replacing the former `Osdu.Schemas` namespace.

## Contributing

Contributions are welcome — see [`CONTRIBUTING.md`](CONTRIBUTING.md) for
development setup, the pull-request process, and commit conventions.

## Security

To report a security vulnerability, follow the process in
[`SECURITY.md`](SECURITY.md). Do not open a public issue.

## License

Licensed under the [Apache License 2.0](LICENSE).
