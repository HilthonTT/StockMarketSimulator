# Changelog: bug fixes, architecture and .NET 10 upgrade

## Upgrade checklist (read before deploying)

1. **Run the new migrations.** In Development they apply automatically on start-up.
   - `GeneralDbContext`, `Fix_OutboxConsumer_Key_And_Cache_Expiry`: primary key of `general.outbox_message_consumers` becomes `(id, name)`; `general.cache_items` gets `expires_on_utc`.
   - `BudgetingDbContext`, `Budget_Concurrency_And_Balance_Constraint`: the check constraint becomes `money_amount >= 0`. The `xmin` row version needs no DDL.
   - `UsersDbContext`, `Remove_Role_Users_Navigation`: renames `users.role_user.users_id` to `user_id`.
2. **Existing mixed-case emails.** Emails are now normalised to lower case on input. Accounts stored with upper-case letters must be lower-cased once in the database, or those users can't log in:
   `UPDATE users.users SET email = lower(email) WHERE email <> lower(email);` (check first for accounts that differ only by case).
3. **Container images** now use the .NET 10 base images.

## API changes

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

## Bugs fixed

### Money and trading (Budgeting)
- **Trades moved only one share's price.** Buying N shares debited the price of one share, and selling credited the price of one. `Transaction.Create` now applies price × quantity.
- **Double spend and overselling.** There was no concurrency control on budgets. An `xmin` row version now serialises concurrent trades per user.
- **A zero balance was rejected.** The check constraint `buying_power > 0` forbade spending everything and referred to a renamed column.
- Sell jobs wrote `BuyStock` audit logs and logged under the buy job's category.
- "Purchased tickers" included positions that had been fully sold, and loaded every transaction synchronously.
- A negative `pageSize` crashed the transactions and audit-log queries.
- The transaction-count query was cached under the purchased-tickers key.

### Stocks
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

### Users
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

### Shared infrastructure
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

### Web UI
- The SignalR connection was never stopped on unmount, so connections leaked on navigation, and the hook read a ref during render.

## Architecture changes

- **One outbox implementation.** The three copy-pasted 160-line jobs became `OutboxProcessorJob` plus three short subclasses.
- **Pure, testable price model** (`GeometricBrownianMotion`) in the Stocks domain.
- **No second DI containers.** `BuildServiceProvider()` was removed from the Quartz and JWT setup in favour of deferred options configuration.
- **One Redis connection** per process, shared by the cache and the rate limiter.
- **Central build configuration:** `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.slnx`, repo-local `dotnet-ef`.
- **A single integration-event serializer**, shared by the publisher and the consumer.
- Removed dead code: the MVC `IdempotentAttribute`, which minimal APIs never used, and unused shadcn components.
- New domain events: `UserProfileChangedDomainEvent` drives cache invalidation.

## Tests

The unit and architecture suites pass (93 tests). New regression tests cover:

- trade totals and insufficient funds;
- spending the entire budget;
- GBM drift, clamping, positivity and the normal sampler;
- email normalisation;
- refresh-token absolute expiry.
