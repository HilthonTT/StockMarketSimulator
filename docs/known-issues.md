# Known issues and follow-ups

These items were identified but intentionally left open, either because they need a product decision or because they belong to another part of the system.

## Build warnings

`dotnet build StockMarketSimulator.slnx` is warning-free, and so is `bun run lint` in `frontend/` (the React Compiler `set-state-in-effect` / `purity` warnings are fixed). `ASPIRE010` is **suppressed** in the AppHost (not fixed): the AppHost keeps `AspireUseCliBundle=false` so contributors and CI don't need the Aspire CLI. To adopt the CLI bundle, install the Aspire CLI, set `AspireUseCliBundle=true` and remove the `NoWarn`/`MSBuildWarningsAsMessages` entries in `StockMarketSimulator.AppHost.csproj`.

## Behaviour and design

- **Assigning the Admin role.** The outbox endpoints (`GET outbox/failures`, `POST outbox/{schema}/redrive`) need the new `ManageOutbox` permission, which only the `Admin` role has. There's no API to grant `Admin`; insert a row into `users.role_user` (`roles_id = 2`) for operators.
- **Rate limiting when Redis drops at runtime.** The global limiter now falls back to an in-process token bucket (same 100-token burst, 50/s refill) when Redis is unreachable, so requests keep flowing. While degraded, limits apply **per instance**, not cluster-wide.
