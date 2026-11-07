# Getting started

## Prerequisites

| Tool | Version | Notes |
| --- | --- | --- |
| .NET SDK | 10.0.100 or later | Pinned in [`global.json`](../global.json) (`rollForward: latestFeature`) |
| Docker | Recent Docker Desktop / Engine | Aspire runs PostgreSQL, Redis, RabbitMQ and Azurite as containers. The integration and functional tests use Testcontainers |
| Bun | 1.x | Package manager for the Next.js web UI (`src/web-ui/bun.lock`) |
| Node.js | 24.x | Runtime for the web UI |

## Run the backend with .NET Aspire

The Aspire AppHost (`src/StockMarketSimulator.AppHost`) starts the infrastructure containers, then the API and the YARP proxy:

```bash
dotnet run --project src/StockMarketSimulator.AppHost
```

The Aspire dashboard URL is printed on start-up. The AppHost:

- creates `stockmarketsimulator-db` (PostgreSQL with pgAdmin), `stockmarketsimulator-redis`, `stockmarketsimulator-rabbitmq` and `stockmarketsimulator-blobs` (Azurite), and injects their connection strings into `web-api`;
- starts `yarp-proxy` once `web-api` is running, and points the proxy at the API's HTTPS endpoint through the `ReverseProxy__Clusters__...__Address` environment variable;
- adds dashboard commands that open Swagger UI, Scalar and ReDoc.

In `Development`, the API applies all EF Core migrations at start-up (`MigrationExtensions.ApplyMigrations`).

## Configuration

`src/Web.Api/appsettings.Development.json` holds working defaults. Keep secrets in [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), not in the file:

```bash
cd src/Web.Api
dotnet user-secrets set "Stocks:ApiKey" "<alpha-vantage-key>"
dotnet user-secrets set "Jwt:Secret" "<at least 32 random characters>"
dotnet user-secrets set "Email:SenderEmail" "<smtp user>"
dotnet user-secrets set "Email:SmtpPassword" "<smtp password>"
```

| Section | Purpose |
| --- | --- |
| `ConnectionStrings:stockmarketsimulator-*` | Database, Redis, blobs and RabbitMQ. Supplied by Aspire when you use the AppHost |
| `Jwt` | Token signing secret, issuer, audience and lifetime |
| `Stocks:ApiUrl` / `Stocks:ApiKey` | Alpha Vantage endpoint and key. The key is never written to logs |
| `Email` | SMTP settings for verification and confirmation emails |
| `StockUpdateOptions` | Real-time feed: `UpdateIntervalInSeconds` and `MaxPercentageChange` per tick (see [price simulation](price-simulation.md)) |
| `MessageBroker:QueueName` | RabbitMQ queue for integration events |
| `Blobs:ContainerName` | Azure Blob container for profile and banner pictures |
| `Database` | EF retry count, command timeout, diagnostics flags |
| `FeatureManagement` | One flag per API area (`UseV1StocksApi`, `UseV1UsersApi`, ...). A disabled flag returns 404 |
| `Cors:AllowedOrigin` | Origin allowed in Development |
| `Sentry:Dsn` | Optional error reporting |

Options classes are validated with FluentValidation at start-up, so missing or invalid settings fail fast.

## Database migrations

Each module owns a `DbContext` and a schema: `users`, `stocks`, `budgeting`, plus `general` for shared tables. The EF CLI is a repo-local tool:

```bash
dotnet tool restore

# add a migration (example: Budgeting)
dotnet ef migrations add <Name> \
  --project src/Modules/Budgeting/Modules.Budgeting.Infrastructure \
  --startup-project src/Web.Api \
  --context BudgetingDbContext \
  --output-dir Database/Migrations

# preview the SQL of a migration range
dotnet ef migrations script <From> <To> --project ... --startup-project src/Web.Api --context ...
```

| Context | Project |
| --- | --- |
| `UsersDbContext` | `src/Modules/Users/Modules.Users.Infrastructure` |
| `StocksDbContext` | `src/Modules/Stocks/Modules.Stocks.Infrastructure` |
| `BudgetingDbContext` | `src/Modules/Budgeting/Modules.Budgeting.Infrastructure` |
| `GeneralDbContext` | `src/Infrastructure` |

## Tests

```bash
dotnet test StockMarketSimulator.slnx
```

| Project | Kind | Needs Docker |
| --- | --- | --- |
| `tests/Domain.UnitTests` | Domain rules (money, transactions, GBM, emails, tokens) | No |
| `tests/Application.UnitTests` | Handlers and factories with NSubstitute | No |
| `tests/ArchitectureTests` | Layer and dependency rules (NetArchTest) | No |
| `tests/Application.IntegrationTests` | Handlers against real containers | Yes |
| `tests/Api.FunctionalTests` | HTTP endpoints through `WebApplicationFactory` | Yes |

## Web UI

```bash
cd src/web-ui
bun install
bun run dev      # http://localhost:3000
bun run build    # production build, including the type check
bun run lint     # ESLint (flat config from eslint-config-next)
```

The UI expects a `.env` file:

```env
AUTH_SECRET=
NODE_TLS_REJECT_UNAUTHORIZED=0
UPSTASH_REDIS_REST_URL=
UPSTASH_REDIS_REST_TOKEN=
```

The live price chart connects to the `/stocks-feed` SignalR hub. The JWT is sent with `accessTokenFactory`, and the API accepts it from the `access_token` query string on that path.
