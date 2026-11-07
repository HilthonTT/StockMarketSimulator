# Known issues and follow-ups

These items were identified but intentionally left out of the bug-fix and upgrade pass, either because they need a product decision or because the change would be large.

## Build warnings still present

| Warning | Where | Suggested action |
| --- | --- | --- |
| `CS0618` Sentry `UseOpenTelemetry` / `AddSentry` obsolete | `Web.Api/Program.cs`, `ServiceDefaults/Extensions.cs` | Move to the `Sentry.OpenTelemetry.Exporter` package and `UseOtlp` |
| `ASPIRE010` CLI bundle disabled | AppHost | Install the Aspire CLI and set `AspireUseCliBundle=true`, or suppress the warning |
| `AV0029` / `AV0030` | API versioning + OpenAPI | Call `MapOpenApi().WithDocumentPerVersion()`, or drop Swashbuckle in favour of the built-in OpenAPI document |
| `CS8618` on private EF constructors | Domain entities | Pre-existing; add `= null!` or `required` where appropriate |
| `CS9113` unread `eventBus` parameter | One handler | Remove the parameter or publish the intended event |
| React Compiler lint warnings (`set-state-in-effect`, `purity`) | web UI | Replace prop-to-state syncing effects with derived state or `key` resets |

## Behaviour and design

- **Integration and functional tests weren't run** in this pass because they need Docker. Run `dotnet test StockMarketSimulator.slnx` with Docker available before releasing.
- **No positions model.** Holdings are rebuilt from the full transaction history, both for the oversell check and for "purchased tickers". A `positions` table updated in the same transaction would make these O(1) and simpler to reason about.
- **`stocks.stocks` has no unique index on `ticker`.** Concurrent first lookups can insert duplicates. Every read is now deterministic and every price update writes all rows, but a unique index plus an upsert would be cleaner. Deduplicate the existing data first.
- **The active ticker set only grows.** Tickers stay active after every client has left. Reference-counting subscriptions in the hub (`JoinGroup`/`LeaveGroup`/`OnDisconnectedAsync`) would bound the feed's work.
- **The refresh-token cleanup job keeps only the newest token per user**, which logs out other devices. Delete only expired tokens, or cap the count per user.
- **Refresh tokens are stored in plain text.** Store a hash.
- **Integration events are defined in Application projects** (e.g. `Modules.Users.Application`). Moving them to a dedicated `*.IntegrationEvents` contracts project per module would make the module contracts explicit.
- **Layer leaks.** The Users Application layer builds Quartz jobs directly (resend verification), Application commands accept ASP.NET `IFormFile`, and Users BackgroundJobs reference the Budgeting domain (`Currency`).
- **Feature targeting reads groups from the client-supplied `X-User-Groups` header**, which is demo-only. Use claims in production.
- **The rate limiter needs Redis.** In the Postgres-cache fallback mode, requests to rate-limited endpoints fail until Redis is reachable.
- **Outbox retries.** A message whose handlers fail after 3 attempts is marked processed with its error. Add alerting on `outbox_messages.error IS NOT NULL`, or a re-drive tool.
