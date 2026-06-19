# Contributing

Thanks for your interest in contributing to `osdu-csharp-samples`! This project
provides runnable C# samples for the `Equinor.OsduCsharpClient` and
`Equinor.Osdu.Schemas` packages against OSDU Wellbore DDMS. Contributions are
welcome via issues and pull requests.

## Reporting issues

- Search [existing issues](https://github.com/equinor/osdu-csharp-samples/issues)
  before opening a new one.
- For security vulnerabilities, **do not** open a public issue — follow
  [`SECURITY.md`](SECURITY.md) instead.

## Development setup

Requires the .NET SDK.

```bash
dotnet build                 # build the samples
dotnet test                  # run tests, if present
osdu-samples list            # list every sample
osdu-samples get-welllog     # run a single sample
```

Each sample is a small, self-contained class meant to read as documentation.
Keep new samples focused and self-contained, and prefer read-only behaviour
unless the sample is explicitly an opt-in write sample (`--write`).

## Pull request process

1. Fork the repository (or create a branch if you have write access) and base
   your work on `main`.
2. Make your change, then build and run the affected samples locally.
3. Open a pull request against `main`.
4. **PR titles must follow [Conventional Commits](https://www.conventionalcommits.org/)**
   (e.g. `feat: add bulk-statistics sample`, `fix: correct welllog id flag`).
5. At least one approving review (including a CODEOWNERS review) is required
   before merging. Direct pushes to `main` are not permitted.
6. Pull requests are merged using **squash merge**.

## License

By contributing, you agree that your contributions will be licensed under the
[Apache License 2.0](LICENSE).
