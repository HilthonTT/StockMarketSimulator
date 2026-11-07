# Build and dependencies

## Target framework and solution

- Every project targets **.NET 10** (`net10.0`), set once in [`Directory.Build.props`](../Directory.Build.props) together with `Nullable`, `ImplicitUsings` and `LangVersion`. Project files no longer repeat these.
- [`global.json`](../global.json) pins the SDK band (`10.0.100`, `rollForward: latestFeature`).
- The solution file is **[`StockMarketSimulator.slnx`](../StockMarketSimulator.slnx)**, the XML solution format. The old `.sln` was removed. The Visual Studio 17.13+/2026, Rider and `dotnet` CLI tooling all support `.slnx`.

```bash
dotnet build StockMarketSimulator.slnx
dotnet test  StockMarketSimulator.slnx
```

## Central Package Management

All NuGet versions live in [`Directory.Packages.props`](../Directory.Packages.props). Project files use `<PackageReference Include="..." />` without a version.

- To bump a package, edit `Directory.Packages.props` only.
- `CentralPackageTransitivePinningEnabled` is on, so a pin also applies when the package arrives transitively. `Microsoft.EntityFrameworkCore.Relational` is pinned for this reason; before, it caused MSB3277 version-conflict warnings.
- Framework-bound packages (ASP.NET Core, EF Core, Npgsql, `Microsoft.Extensions.*`) stay on the **10.x** line to match the target framework.

Notable major upgrades and the code changes they required:

| Package | From | To | Code impact |
| --- | --- | --- | --- |
| Quartz | 3.14 | 4.7 | `IJob.Execute` returns `ValueTask` and takes a `CancellationToken`. Typed `ScheduleJob<T>` registration. `SetJobData` became `UsingJobData`. Typed `JobDataMap` getters replaced (see `JobDataMapExtensions`) |
| Aspire | 9.3 | 13.6 | AppHost uses `Sdk="Aspire.AppHost.Sdk/13.6.4"`. `Aspire.Hosting.AppHost` is implicit. `ExecuteCommandResult.ErrorMessage` became `Message` |
| Swashbuckle / Microsoft.OpenApi | 8 / 1.x | 10 / 2.x | Namespace `Microsoft.OpenApi`. Security requirements use `OpenApiSecuritySchemeReference`. `WithOpenApi()` removed from endpoints, since .NET 10 generates the metadata |
| Testcontainers | 4.4 | 4.16 | Builders take the image in the constructor (images pinned in the test factories) |
| Scrutor, FluentValidation, Polly, RabbitMQ.Client, MailKit, Sentry, OpenTelemetry, Asp.Versioning, xUnit runner, Test SDK, ... | | latest stable | No code changes |

`OpenTelemetry.Instrumentation.EntityFrameworkCore` has no stable release yet and stays on its beta line.

Sentry tracing uses **`Sentry.OpenTelemetry.Exporter`** (the replacement for the obsolete `Sentry.OpenTelemetry` integration): `SentryOptions.UseOtlp()` in `Web.Api/Program.cs` and `AddSentryOtlpExporter(dsn)` in ServiceDefaults, both only when `Sentry:Dsn` is set. **`Asp.Versioning.OpenApi`** wires the built-in OpenAPI generator to API versioning.

The backend builds with **zero warnings**. `ASPIRE010` (Aspire CLI bundle disabled) is suppressed in the AppHost on purpose; see [known issues](known-issues.md).

## Tools

The EF Core CLI is a repo-local tool in [`dotnet-tools.json`](../dotnet-tools.json), pinned to the EF Core version:

```bash
dotnet tool restore
dotnet ef --version
```

## Docker images

`src/Web.Api/Dockerfile` and `src/Yarp.Proxy/Dockerfile` build on `mcr.microsoft.com/dotnet/sdk:10.0` and run on `mcr.microsoft.com/dotnet/aspnet:10.0`. Restore needs `global.json`, `Directory.Build.props` and `Directory.Packages.props`, so the build copies them first. The previous Web.Api Dockerfile restored with only its own `.csproj`, which can't work without the referenced projects.

## Web UI

| Package | From | To | Code impact |
| --- | --- | --- | --- |
| next / eslint-config-next | 15.5 / 15.3 | 16.4 | `lint` script is now `eslint .`. Native flat ESLint config (no `FlatCompat`). New React Compiler lint rules |
| zod | 3 | 4 | `required_error`/`invalid_type_error` became `error`. `z.coerce.number<number>()` for form typing |
| recharts | 2 | 3 | Custom tooltip typed with `TooltipContentProps` |
| react-day-picker | 8 | 10 | `calendar.tsx` ported to v9+ class names and the `Chevron` component. `initialFocus` became `autoFocus` |
| @trpc/* | 11.1 | 11.25 | Prefetching infinite queries needs `getNextPageParam`. Invalidation uses `infiniteQueryFilter` |
| typescript | 5.8 | 6.0 | 7.x isn't supported by `typescript-eslint` yet |
| react, @tanstack/*, lucide-react, motion, @microsoft/signalr, uuid, ... | | latest | No code changes |

`next-auth` stays on the v5 beta (`5.0.0-beta.32`), because its `latest` npm tag still points at v4. The unused `chart.tsx`/`resizable.tsx` shadcn components were removed, along with `react-resizable-panels` and `@eslint/eslintrc`.
