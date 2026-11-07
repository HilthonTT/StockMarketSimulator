# Changelog

## Front end overhaul, CI/CD and repository layout

### Repository layout

- The Next.js app moved from `src/web-ui` to a top-level **`frontend/`**, so `src/` and `tests/` hold only .NET code. It has its own README, `.env.example` and Dockerfile (standalone output).
- New **`.github/`**: CI and CD workflows, Dependabot, issue and pull request templates. See [CI/CD](ci-cd.md).

### Navigation and pages

- An app shell replaces the top navbar: a collapsible sidebar (Dashboard, Markets, Portfolio, History, Profile, How it works), a top bar with stock search, live-feed status and cash balance, and a ticker tape of holdings and watchlist prices.
- **Ctrl/⌘+K** (or `/`) opens a command palette that searches stocks by ticker or name and jumps to pages.
- New pages:
  - **Markets**: searchable company list with a watchlist.
  - **Stock detail** (`/stocks/[ticker]`): live chart with your average cost, your position, an order ticket, and your trades in that stock.
  - **Portfolio**: holdings at live prices, total return and allocation.
  - **History**: filter by ticker and date range, with presets.
- **Dashboard**: account value and all-time return against the $5,000 starting cash, buying power, invested value and unrealized P&L, a live chart of a holding, top holdings and recent activity. New accounts get a first-trade guide instead.
- **Order ticket** (shared by the stock page and the trade dialog):
  - Buy/sell toggle, quantity stepper and presets (Max, 25% / 50% / All).
  - Shows estimated cost or proceeds, cash after the order, and shares after.
  - Validates against your cash and the shares you own before submitting.
- Profile, login and register were redesigned. The empty About page now explains how the simulation works. The non-functional Contact form was removed and `/contact` redirects to `/about`.

### Bugs fixed

| Bug | Fix |
| --- | --- |
| Changing your password always reported success but did nothing: the UI called `users/{id}/change-password`, while the route is `authentication/{id}/change-password` (the 404 was swallowed) | Correct route; mutations now treat 404 as an error |
| Buy/sell/update calls that returned 404 were shown as successful | `fetchFromApi({ throwOnNotFound })` for mutations |
| Each transaction row and the chart left the SignalR group for its ticker on unmount, which unsubscribed every other component showing that ticker; each section also opened its own connection | One app-wide connection with reference-counted group subscriptions, re-joined after reconnects |
| The transaction count rendered wrong (CountUp `decimal="2"` set the decimal **separator** to "2", so 5 showed as "5200") | Count-up removed |
| The "change" column showed a dollar difference with a `%` sign | Positions show real P&L in $ and % |
| Searching for anything that wasn't an exact ticker did nothing (the modal silently returned `null`) | Live search with results, plus an "open ticker" fallback and a not-found state |
| The trade modal never opened when the external "more info" link couldn't be resolved | The link is optional, with a Yahoo Finance fallback |
| The mobile menu showed "Log in" / "Start Gambling!" to signed-in users and had no log out | New sidebar and user menu |
| Logging out from the navbar didn't revoke API refresh tokens | The logout action revokes tokens, then signs out |
| The date filter showed "this month" while no filter was applied; picking an end date excluded trades made that day | "All time" by default; ranges cover whole days |
| Timestamps from the API (UTC without an offset) were shown as local time | `parseUtc` |
| Every tRPC call failed when Upstash wasn't configured | Rate limiting is skipped without Upstash credentials |
| Non-JSON error responses crashed the API client; users saw raw messages like `Error: Title - Detail` | Readable problem-details messages and status-specific tRPC codes |
| Auth layout overflowed (`w-3/4` + `w-1/2`) | Two-column grid |
| Google/GitHub sign-in were configured but couldn't produce a working session (no API token), and next-auth pointed errors at a missing `/error` page | Credentials only; errors go to `/login` |
| Avatar/banner fallbacks used relative paths that broke on nested routes; the upload dialogs could "save" without a file | One upload dialog with type and size checks |
| React Compiler lint warnings (`set-state-in-effect`, `purity`) | Fixed; `bun run lint` is clean |

Unused dependencies removed: `react-countup`, `react-icons`, `embla-carousel-react`, `axios`, `motion`, `@tanstack/react-table`, `@trpc/next`, `@trpc/react-query`.

## Follow-up pass: known issues resolved

### Upgrade checklist (read before deploying)

1. **Run the new migrations** (automatic in Development):
   - `BudgetingDbContext`, `Add_Positions`: creates `budgeting.positions` (unique `user_id, ticker`), upper-cases `budgeting.transactions.ticker`, and **backfills** positions by replaying each user's trades in order with the average-cost rules.
   - `StocksDbContext`, `Unique_Stock_Ticker`: **deletes duplicate `stocks.stocks` rows** (keeps the most recently written row per upper-cased ticker), upper-cases tickers, then makes `ix_stocks_ticker` unique.
   - `UsersDbContext`, `Hash_Refresh_Tokens_And_Outbox_Permission`: replaces `refresh_tokens.token` with `token_hash` (SHA-256 hex) and **deletes every existing refresh token**, so all users must sign in again. Adds the `ManageOutbox` permission and grants it to `Admin`.
2. **Drain the RabbitMQ queue first.** Integration events moved to `Modules.Users.IntegrationEvents` / `Modules.Budgeting.IntegrationEvents`, so their CLR type names changed. Messages published by the previous version can't be deserialised and are rejected without requeue.
3. **Feature targeting ignores `X-User-Groups`.** Groups now come from the user's role claims and an optional `groups` claim only.

### API changes

| Change | Before | After |
| --- | --- | --- |
| **New** `GET api/v1/users/{userId}/positions` | | `200` with `[{ "ticker": "AAPL", "quantity": 3, "averageCost": 105.0, "totalCost": 315.0 }]`, open positions only (quantity > 0), ordered by ticker. Same rules as `users/{userId}/budget`: `Read` + `Write` permissions, flag `UseV1UsersApi`, rate-limited, `403` for another user's id |
| **New** `GET api/v1/outbox/failures` | | `200` with `[{ "schema", "failedCount", "oldestFailedOnUtc" }]`. Requires `ManageOutbox` |
| **New** `POST api/v1/outbox/{schema}/redrive` | | `200` `{ "schema", "redriven" }`; `404` for an unknown schema. Requires `ManageOutbox` |
| Buy / sell | ticker stored as sent | ticker upper-cased. Selling more than the position holds still returns `400 Transactions.InsufficientStock` |
| Buy (first buy of a ticker racing another) | | `409 Budget.ConcurrentModification` (same as other trade races) |
| Login / refresh | | Response shape unchanged; the refresh token is now only valid if its SHA-256 hash is stored |
| `/health` | | New `outbox` entry; `Degraded` (HTTP 200) while any outbox has failed messages |
| OpenAPI | `openapi/v1.json` from a single document | One document per API version through `Asp.Versioning.OpenApi` (`openapi/v1.json` unchanged for v1) |
| SignalR `/stocks-feed` | | Unchanged contract (`JoinGroup`, `LeaveGroup`, `ReceiveStockPriceUpdate`) |

### Changes

- **Positions model.** `Position` (Budgeting domain) keeps quantity and cost basis per user and ticker, updated in the same `SaveChanges` as the trade and the budget. Sells and "purchased tickers" read it instead of replaying all transactions; the top performer uses it for its cost basis.
- **Unique stock tickers.** `stocks.stocks.ticker` is unique and first lookups upsert (`ON CONFLICT (ticker) DO UPDATE`).
- **Reference-counted feed subscriptions.** `ActiveTickerManager` tracks subscriptions per SignalR connection (`JoinGroup`, `LeaveGroup`, `OnDisconnectedAsync`) plus a 5-minute lease for REST quotes. The feed's own reads no longer renew the lease, so the active set shrinks when nobody is watching.
- **Refresh-token cleanup** deletes only expired tokens and caps live tokens at 10 per user. Signing in on a second device no longer logs out the first.
- **Hashed refresh tokens** (SHA-256). Only the hash is stored; lookups hash the presented token.
- **Integration-event contract projects:** `Modules.Users.IntegrationEvents`, `Modules.Budgeting.IntegrationEvents`.
- **Layer fixes.** `IUserJobScheduler` (implemented in `Modules.Users.BackgroundJobs`) replaces Quartz calls in the Users Application layer and the Users event handlers; `Modules.Users.Application` no longer references `Modules.Users.BackgroundJobs`. Picture-upload commands take `Stream` + file name + content type instead of `IFormFile`. Users BackgroundJobs uses `BudgetCurrencies.Default` from `Modules.Budgeting.Api` instead of the Budgeting domain's `Currency`.
- **Feature targeting from claims** (role claims + `groups` claim).
- **Rate limiter without Redis.** In Postgres-cache fallback mode the global policy uses an in-process token bucket; if Redis fails at runtime, `RedisFallbackRateLimiter` falls back to it instead of failing the request.
- **Outbox re-drive and alerting.** `OutboxAdministration` lists and resets failed messages; `OutboxHealthCheck` reports them and `OutboxHealthLogPublisher` logs a warning every 30 s while any exist.
- **Zero build warnings.** Sentry tracing moved to `Sentry.OpenTelemetry.Exporter` (`UseOtlp` + `AddSentryOtlpExporter`, only when `Sentry:Dsn` is set); OpenAPI documents per version (`AddApiVersioning().AddOpenApi()`, `MapOpenApi().WithDocumentPerVersion()`); `= null!` on EF-only constructors; the unread `eventBus` parameter was removed (nothing consumes a follower event yet); `ASPIRE010` suppressed (see [known issues](known-issues.md)).

### Tests

- Integration and functional suites now run (they need Docker). They were broken before: the functional tests posted to a non-existent route, used passwords the validator rejects, and the integration tests resolved scoped handlers from the root provider.
- New coverage: `Position` cost basis, positions endpoint shape/ownership/oversell, refresh-token hashing (unit + end-to-end), cleanup job (expiry, per-user cap, other devices kept), active-ticker reference counting and lease expiry, claims-based targeting groups, rate-limiter fallback, module contract architecture rules.

---

## Previous pass: bug fixes, architecture and .NET 10 upgrade

### Upgrade checklist (read before deploying)

1. **Run the new migrations.** In Development they apply automatically on start-up.
   - `GeneralDbContext`, `Fix_OutboxConsumer_Key_And_Cache_Expiry`: primary key of `general.outbox_message_consumers` becomes `(id, name)`; `general.cache_items` gets `expires_on_utc`.
   - `BudgetingDbContext`, `Budget_Concurrency_And_Balance_Constraint`: the check constraint becomes `money_amount >= 0`. The `xmin` row version needs no DDL.
   - `UsersDbContext`, `Remove_Role_Users_Navigation`: renames `users.role_user.users_id` to `user_id`.
2. **Existing mixed-case emails.** Emails are now normalised to lower case on input. Accounts stored with upper-case letters must be lower-cased once in the database, or those users can't log in:
   `UPDATE users.users SET email = lower(email) WHERE email <> lower(email);` (check first for accounts that differ only by case).
3. **Container images** now use the .NET 10 base images.

### API changes

| Change | Before | After |
| --- | --- | --- |
| Read audit logs | `POST users/{id}/auditlogs` | `GET users/{id}/auditlogs`. The parameters were already query-string values |
| Delete audit log | anonymous; feature flag `UseV1UsersApi` | requires `Read` + `Write` permissions; flag `UseV1BudgetingApi` |
| Delete file | any user could delete any blob | only the uploader (blob `owner_id` metadata). Unknown or foreign files return 404 |
| Update user | `{ username }` | `{ username, hasPublicProfile? }`. Users can now make their profile public, which following requires |
| Get user by id | always returned the email | email is empty unless you request your own profile |
| Buy / sell | | can now return **409** `Budget.ConcurrentModification` when two trades race; `Budget.InsufficientBuyingPower` when funds are short |
| `Problem`-type errors | HTTP 500 | HTTP 400 |
| Exception responses | body status could disagree with the HTTP status; 500s leaked exception messages | the HTTP status matches the body; 500s return a generic message |
| `IBudgetingApi.GetTransactionsByUserIdAsync` | | `TransactionApiResponse` now includes `Quantity` |
| SignalR `/stocks-feed` | WebSocket connections from browsers were rejected (token not read) | JWT accepted from the `access_token` query string |

### Bugs fixed

#### Money and trading (Budgeting)
- **Trades moved only one share's price.** Buying N shares debited the price of one share, and selling credited the price of one. `Transaction.Create` now applies price × quantity.
- **Double spend and overselling.** There was no concurrency control on budgets. An `xmin` row version now serialises concurrent trades per user.
- **A zero balance was rejected.** The check constraint `buying_power > 0` forbade spending everything and referred to a renamed column.
- Sell jobs wrote `BuyStock` audit logs and logged under the buy job's category.
- "Purchased tickers" included positions that had been fully sold, and loaded every transaction synchronously.
- A negative `pageSize` crashed the transactions and audit-log queries.
- The transaction-count query was cached under the purchased-tickers key.

#### Stocks
- **The first quote for a ticker always returned 404.** After fetching from the API, the service returned the empty database value.
- **The price feed wasn't a random walk,** and trades were priced differently from the feed. See [price simulation](price-simulation.md).
- GBM units were wrong: daily parameters were treated as annual, there was a fixed `dt`, an arithmetic Euler step, and the configured clamp was ignored.
- **The Alpha Vantage API key was written to logs.**
- Tickers and search terms weren't URL-encoded (e.g. "AT&T", or parameter injection).
- The stored price was the bar's **high** instead of its close, and "latest" relied on dictionary order.
- Every search with a term called the external API and inserted duplicate rows.
- Ticker short links failed with 500 under concurrency. The retry loop's last-attempt check could never be true.
- Short-code cache keys shared one keyspace with other cache entries.
- The top performer used the single most expensive buy as cost basis. It now uses the quantity-weighted average of open positions.
- One failing ticker cancelled the whole feed tick. One failing ticker rolled back the whole price-refresh batch, which also had no ordering and could starve other stocks.
- Tickers were case-sensitive, and subscribing over SignalR alone never produced updates.
- `IStocksApi` returned an arbitrary row when a ticker had duplicates.

#### Users
- **The recent-followers query always failed** (`u.name` doesn't exist; `created_on_utc` was ambiguous).
- **Nobody could be followed.** `HasPublicProfile` could never become true.
- **Any user could delete any file,** including other users' avatars.
- **Changing the password didn't revoke refresh tokens,** and rotation slid the expiry forever.
- The shared static `Role.Registered` instance was attached to every request's `DbContext`, collecting users across requests through the `Role.Users` navigation.
- The authorization policy provider released a semaphore it might not hold (`SemaphoreFullException`), and mutated shared options without thread safety.
- Emails were case-sensitive, so duplicate accounts and failed logins were possible.
- Two concurrent follow requests caused a 500.
- The user cache wasn't cleared when the profile picture, banner or visibility changed.
- `UserEmailVerifiedJob` reported the wrong job name.

#### Shared infrastructure
- **An event with several handlers broke idempotency.** The consumer table was keyed on the event id only.
- **Negative idempotency lookups were cached for 5 minutes,** so a retried event could be handled twice.
- **One undeserialisable outbox message blocked the outbox forever**; dispatch had unbounded parallelism.
- **The RabbitMQ consumer looped on poison messages** (never acked or rejected), and reconnected for every single message.
- **Integration events were deserialised with `TypeNameHandling.All`** from the broker, which can instantiate arbitrary types. An allow-list binder now restricts this.
- `EventBus` (a singleton) shared one RabbitMQ channel across threads without synchronisation, and raced on lazy initialisation.
- The Postgres cache fallback queried a table that doesn't exist (`general.cache` instead of `cache_items`), had a stray `$` in its DELETE statement, and ignored expirations.
- `ExceptionHandlingDecorator` didn't await the inner handler, so it never caught asynchronous exceptions.
- Both logging middlewares disposed their log scope or activity before the request finished. The request activity source was never registered with OpenTelemetry.
- `UseExceptionHandler` was registered after other middleware.
- `GlobalExceptionHandler` left the HTTP status at 500 even when the body said 400 or 401.
- `MigrationExtensions` disposed scope-owned `DbContext`s.
- `UserTargetingContext` never stored its per-request cache.
- `Ensure.GreaterThanZero` reported "greater than or equal to zero".
- The Web.Api Dockerfile couldn't restore (it copied only its own project file).
- The YARP proxy pointed at a hard-coded port instead of the Aspire-assigned API endpoint.

#### Web UI
- The SignalR connection was never stopped on unmount, so connections leaked on navigation, and the hook read a ref during render.

### Architecture changes

- **One outbox implementation.** The three copy-pasted 160-line jobs became `OutboxProcessorJob` plus three short subclasses.
- **Pure, testable price model** (`GeometricBrownianMotion`) in the Stocks domain.
- **No second DI containers.** `BuildServiceProvider()` was removed from the Quartz and JWT setup in favour of deferred options configuration.
- **One Redis connection** per process, shared by the cache and the rate limiter.
- **Central build configuration:** `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.slnx`, repo-local `dotnet-ef`.
- **A single integration-event serializer**, shared by the publisher and the consumer.
- Removed dead code: the MVC `IdempotentAttribute`, which minimal APIs never used, and unused shadcn components.
- New domain events: `UserProfileChangedDomainEvent` drives cache invalidation.

### Tests

The unit and architecture suites pass (93 tests). New regression tests cover:

- trade totals and insufficient funds;
- spending the entire budget;
- GBM drift, clamping, positivity and the normal sampler;
- email normalisation;
- refresh-token absolute expiry.
