# Known issues and follow-ups

These items were identified but intentionally left open, either because they need a product decision or because they belong to another part of the system.

## Build warnings

`dotnet build StockMarketSimulator.slnx` is warning-free, and so is `bun run lint` in `frontend/` (the React Compiler `set-state-in-effect` / `purity` warnings are fixed). `ASPIRE010` is **suppressed** in the AppHost (not fixed): the AppHost keeps `AspireUseCliBundle=false` so contributors and CI don't need the Aspire CLI. To adopt the CLI bundle, install the Aspire CLI, set `AspireUseCliBundle=true` and remove the `NoWarn`/`MSBuildWarningsAsMessages` entries in `StockMarketSimulator.AppHost.csproj`.

## Behaviour and design

- **Assigning the Admin role.** The outbox endpoints (`GET outbox/failures`, `POST outbox/{schema}/redrive`) need the new `ManageOutbox` permission, which only the `Admin` role has. There's no API to grant `Admin`; insert a row into `users.role_user` (`roles_id = 2`) for operators.
- **Rate limiting when Redis drops at runtime.** The global limiter now falls back to an in-process token bucket (same 100-token burst, 50/s refill) when Redis is unreachable, so requests keep flowing. While degraded, limits apply **per instance**, not cluster-wide.
- **Whole units only.** Positions hold whole shares or coins, so crypto priced far above the starting cash (BTC) isn't listed. Fractional quantities would need decimal quantities in `Position`, `Transaction` and `Order`, plus a migration.
- **News is in memory.** `MarketNewsStore` keeps the last 100 headlines per API instance and loses them on restart. The price effect of news is persisted, like every other tick.
- **Account value chart is approximate.** The API keeps no account snapshots, so the client values your *current* cash and holdings at each historical price. It doesn't replay past trades.
- **Integration events have no dead-letter queue.** After 5 failed attempts a message is logged with its body and dropped. Changing the arguments of the existing durable queue to add a DLX would make `QueueDeclare` fail on existing brokers.
- **Budgeting integration events** still get fresh ids per publish. Consumers deduplicate per delivery, not per domain event as the Users module now does.
- **Rate limiting behind a proxy.** Forwarded headers aren't trusted, so anonymous calls through YARP or the Next.js server share the proxy's bucket. Authenticated calls are partitioned per user.
- **Seeded session on existing databases.** A database that existed before the market-universe migration starts every listing's session at its current price, so day changes begin at 0%. Fresh installs derive the session from the back-filled history.
