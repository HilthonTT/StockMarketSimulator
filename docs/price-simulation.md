# Price simulation

The simulator has its own market. A built-in universe of **30 stocks and 10 crypto assets** is listed at start-up and ticked every second. No external API is needed. Symbols outside the universe can still be quoted from [Alpha Vantage](https://www.alphavantage.co/) if you configure `Stocks:ApiKey`; from then on they are simulated the same way.

## Market universe

`Modules.Stocks.Domain.Services.MarketCatalog` lists every built-in asset with:

| Field | Meaning |
| --- | --- |
| `Ticker`, `Name`, `Sector` | e.g. `NVDA`, NVIDIA Corp., Technology. Crypto tickers look like `ETH-USD` |
| `AssetClass` | `stock` or `crypto`. Crypto trades in whole units like stocks, so coins priced far above the $5,000 starting cash (such as BTC) aren't listed |
| `SeedPrice` | First price when the listing is created. After that, every price is simulated |
| `DailyDrift` (μ), `DailyVolatility` (σ) | GBM parameters per trading day |
| `Beta` | Sensitivity to the shared market factor (see below) |

`MarketSeeder` (a hosted service) upserts the listings into `stocks.stocks`, makes them searchable in `stocks.stock_search_results`, and back-fills history for listings that have none. It's idempotent and retries until the schema exists.

The **SIM 30** index is price-weighted, like the Dow: the sum of the stock prices divided by a fixed divisor, chosen so the index starts at 1,000.

## One tick

`StocksFeedUpdater` (Quartz, every `UpdateIntervalInSeconds`) simulates every built-in listing, plus any other symbol that has a SignalR subscriber or was quoted over REST in the last 5 minutes (`ActiveTickerManager`). Each tick:

1. **Market factor.** One standard normal draw `M` is shared by all assets. Asset *i* uses `Zᵢ = βᵢ·M + √(1−βᵢ²)·εᵢ`, so stocks with a high beta rise and fall together, while crypto (β ≈ 0.3) mostly follows its own path.
2. **GBM step** (`GeometricBrownianMotion.NextPrice`), clamped to `±MaxPercentageChange` per tick:

   $$P_{t+\Delta t} = P_t \cdot \exp\left(\left(\mu - \tfrac{\sigma^2}{2}\right)\Delta t + \sigma\sqrt{\Delta t}\,Z\right)$$

   Δt is the tick length in trading days (`UpdateIntervalInSeconds / 6.5h`). Prices are rounded to cents above $1, and to 4 decimals below $1 so cheap coins keep moving.
3. **News shocks.** On average `NewsEventsPerMinute` events per minute hit a random listing: a headline such as "Analysts upgrade NVIDIA Corp. to Buy" and a jump of 1–3% for stocks or 2–6% for crypto. News bypasses the per-tick clamp. It's kept in memory (`MarketNewsStore`, last 100), served by `GET stocks/news`, and broadcast to every client as `ReceiveMarketNews`.
4. **Sessions.** A simulated trading day is a UTC calendar day. On the first tick of a new day, the last price becomes `previous_close`, and `open_price`, `day_high` and `day_low` restart. Day change is always measured against the previous close.
5. **Persist and broadcast.** All prices and session statistics are written in **one** `UPDATE … FROM unnest(...)`. The current one-minute OHLC bar of every ticker is upserted into `stocks.price_bars`. Then `ReceiveStockPriceUpdate { ticker, price, open, previousClose, dayHigh, dayLow, timestamp }` goes to each ticker's SignalR group.

Persisting every step means the next tick continues from the new price (a real random walk), and buys, sells and order fills use exactly the price on screen.

## Price history

- **Live bars:** one per minute per ticker, written by the feed.
- **Back-fill:** a listing without history gets 89 days of hourly bars plus the last 24 hours in 5-minute bars (`PriceHistoryGenerator`). The generator builds a GBM path forward and then rescales it so it ends exactly at the current price; rescaling keeps the returns and so the volatility.
- **Query:** `GET stocks/{ticker}/history?range=1D|1W|1M|3M` re-buckets the bars with `date_bin` to 5 min, 30 min, 2 h or 8 h (first open, max high, min low, last close).

## Orders

Limit and stop orders are filled by `ProcessPendingOrdersJob` every 2 seconds. It checks pending orders against current prices and fills each triggered order in its own unit of work, at the market price at that moment:

| Order | Fills when |
| --- | --- |
| Limit buy | price ≤ limit |
| Limit sell | price ≥ limit |
| Stop buy | price ≥ stop |
| Stop sell (stop-loss) | price ≤ stop |

Cash and shares aren't reserved while an order waits. If they fall short when it triggers, the order is **rejected** with the reason.

## Tuning

```json
"StockUpdateOptions": {
  "UpdateIntervalInSeconds": 1,
  "MaxPercentageChange": 0.002,
  "NewsEventsPerMinute": 0.25
}
```

To change how assets behave, edit their μ, σ and β in `MarketCatalog`. Symbols outside the catalog use `SimpleStockVolatilityProvider` and a beta of 0.6.
