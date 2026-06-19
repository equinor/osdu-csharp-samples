# OSDU C# Samples

Runnable, focused examples of using the [`Equinor.OsduCsharpClient`][client] and
[`Equinor.Osdu.Schemas`][schemas] packages against OSDU — centred on **Wellbore
DDMS well logs**. Each sample is a small, self-contained class you can read as
documentation and run on its own.

[client]: https://github.com/equinor/osdu-csharp-client
[schemas]: https://github.com/equinor/osdu-csharp-schemas

## Samples

| Name | Description | Writes? |
|---|---|---|
| `service-info` | Print Wellbore DDMS service info (`/about`). | |
| `search-welllogs` | Search for WellLog records by kind. | |
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

Authentication uses interactive MSAL by default (browser on first run, then
silent renewal from cache). Read samples need only the `Osdu` section plus
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

`Equinor.OsduCsharpClient` and `Equinor.Osdu.Schemas` are published to GitHub
Packages. `nuget.config` declares the `equinor-github` source; add credentials
(a PAT with `read:packages`) to your **user-level** NuGet config:

```sh
dotnet nuget add source https://nuget.pkg.github.com/equinor/index.json \
  --name equinor-github --username <you> --password <PAT> --store-password-in-clear-text
```

## Status

> This project targets **`Equinor.OsduCsharpClient` 1.1.0** (which adds Wellbore
> DDMS Parquet bulk-data support) and **`Equinor.Osdu.Schemas` 0.2.1**.

## Contributing

Contributions are welcome — see [`CONTRIBUTING.md`](CONTRIBUTING.md) for
development setup, the pull-request process, and commit conventions.

## Security

To report a security vulnerability, follow the process in
[`SECURITY.md`](SECURITY.md). Do not open a public issue.

## License

Licensed under the [Apache License 2.0](LICENSE).
