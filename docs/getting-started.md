# Getting started

## Prerequisites

| Tool | Version | Notes |
| --- | --- | --- |
| .NET SDK | 10.0.100 or later | Pinned in [`global.json`](../global.json) (`rollForward: latestFeature`) |
| Docker | Recent Docker Desktop / Engine | Aspire runs PostgreSQL, Redis, RabbitMQ and Azurite as containers. The integration and functional tests use Testcontainers |
| Bun | 1.x | Package manager for the Next.js front end (`frontend/bun.lock`) |
| Node.js | 24.x | Runtime for the front end |

## Run it locally

You need Docker running. No API keys, SMTP server or `.env` file are required in Development.

### 1. Start the backend

```bash
dotnet tool restore
dotnet run --project src/StockMarketSimulator.AppHost
```

The Aspire AppHost (`src/StockMarketSimulator.AppHost`):

- starts PostgreSQL (with pgAdmin), Redis, RabbitMQ and Azurite as containers, and injects their connection strings into the API;
- runs the API on **http://localhost:5000** (and https://localhost:7081), then the YARP proxy;
- prints the URL of the Aspire dashboard (logs, traces, and shortcuts to Swagger UI, Scalar and ReDoc).

On start-up in `Development`, the API:

1. applies all EF Core migrations;
2. lists the built-in market universe (30 stocks, 10 crypto assets) and back-fills 90 days of price history. This takes a few seconds on the first run;
3. starts the live price feed, news events and the order-fill job.

### 2. Start the web client

```bash
cd frontend
bun install
bun run dev        # http://localhost:3000
```

Without a `.env`, the client uses `http://localhost:5000` for the API and a development-only auth secret. Copy `.env.example` to `.env` to override them.

### 3. Sign up and trade

1. Open http://localhost:3000 and register.
2. `Users:AutoVerifyEmail` is `true` in `appsettings.Development.json`, so the account is verified at once and gets its $5,000 budget. Emails are written to the API log because no SMTP server is configured.
3. Sign in and trade.

### Troubleshooting

| Symptom | Fix |
| --- | --- |
| "The trading server is unreachable" in the client | The API isn't running on `http://localhost:5000`. Check the Aspire dashboard, or set `NEXT_PUBLIC_SERVER_URL` |
| Containers don't start | Start Docker Desktop first. The first run pulls the images, which takes a few minutes |
| Port 5000 is busy (macOS AirPlay Receiver) | Change `applicationUrl` in `src/Web.Api/Properties/launchSettings.json` and set `NEXT_PUBLIC_SERVER_URL` |
| Trading says "budget not found" right after sign-up | The budget is created by an integration event a few seconds after verification; refresh. If it persists, check `Users:AutoVerifyEmail` and the RabbitMQ container |

### Optional: real quotes

Prices for the built-in universe are always simulated. To also quote other symbols, add an [Alpha Vantage](https://www.alphavantage.co/support/#api-key) key:

```bash
dotnet user-secrets --project src/Web.Api set "Stocks:ApiKey" "<key>"
```

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
| `Email` | SMTP settings for verification and confirmation emails. With `SenderEmail` or `SmtpPassword` empty, emails are written to the log instead |
| `StockUpdateOptions` | Real-time feed: `UpdateIntervalInSeconds`, `MaxPercentageChange` per tick and `NewsEventsPerMinute` (see [price simulation](price-simulation.md)) |
| `App:PublicApiUrl` | **Required.** Public base URL of the API, used to build email-verification links (`http://localhost:5000` in Development) |
| `Users:AutoVerifyEmail` | Verify new accounts immediately. `true` in Development only; never enable in production |
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

## Front end

```bash
cd frontend
cp .env.example .env   # optional in development
bun install
bun run dev        # http://localhost:3000
bun run build      # production build
bun run lint       # ESLint (flat config from eslint-config-next)
bun run typecheck  # tsc --noEmit
```

Every variable is documented in `frontend/.env.example`. In development none is required; in production `AUTH_SECRET` is. Upstash rate limiting is optional and switched off when its variables are empty. See [frontend/README.md](../frontend/README.md) for the pages and code layout.

The live price chart connects to the `/stocks-feed` SignalR hub. The JWT is sent with `accessTokenFactory`, and the API accepts it from the `access_token` query string on that path.
