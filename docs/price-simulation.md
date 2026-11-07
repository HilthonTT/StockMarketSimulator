# Price simulation

Between real quotes from Alpha Vantage, prices move with a **Geometric Brownian Motion (GBM)** model, and every tick is pushed to clients over SignalR.

## Model

`Modules.Stocks.Domain.Services.GeometricBrownianMotion.NextPrice` applies the exact GBM step:

$$
P_{t+\Delta t} = P_t \cdot \exp\left(\left(\mu - \tfrac{\sigma^2}{2}\right)\Delta t + \sigma\sqrt{\Delta t}\,Z\right), \qquad Z \sim \mathcal{N}(0, 1)
$$

| Symbol | Meaning | Source |
| --- | --- | --- |
| μ | daily drift | `SimpleStockVolatilityProvider` (per ticker, with a default) |
| σ | daily volatility | `SimpleStockVolatilityProvider` |
| Δt | step length in **trading days** | `UpdateIntervalInSeconds / (6.5 × 3600)` |
| Z | standard normal draw | Box–Muller transform (`SampleStandardNormal`) using `Random.Shared` |

Then:

- the relative change of one step is clamped to `±StockUpdateOptions:MaxPercentageChange`;
- the result is rounded to cents and never drops below 0.01.

The function is pure (the random draw is passed in), so `tests/Domain.UnitTests/Stocks/GeometricBrownianMotionTests.cs` covers drift, clamping, positivity and the distribution of the normal sampler.

## Feed loop

`StocksFeedUpdater` (Quartz, every `UpdateIntervalInSeconds`):

1. Reads the active tickers from `ActiveTickerManager`. A ticker becomes active when it's quoted over REST or when a client calls the hub's `JoinGroup`.
2. For each ticker, with up to 5 in parallel and **errors isolated per ticker**:
   - load the latest stored price;
   - compute the next GBM price;
   - **persist it** (`IStockService.UpdatePriceAsync`);
   - broadcast `ReceiveStockPriceUpdate` to the ticker's SignalR group.

Persisting each step matters for two reasons:

- the next tick continues from the new price, so the series is a real random walk instead of noise around a fixed anchor;
- buys and sells, which price through `IStocksApi`, use the same price the user sees on the chart.

Tickers are normalised to upper case everywhere (REST, hub groups, storage, `IStocksApi`), so `aapl` and `AAPL` are the same stock.

## Real quotes

`StockService.GetLatestStockPriceAsync` serves the stored price. When the ticker is unknown, it fetches the latest **close** of the most recent 15-minute bar from Alpha Vantage and stores it. `UpdateLatestStockPriceJob` refreshes stored prices that haven't changed for a while; prices the feed is actively updating aren't stale, so they aren't reset.

## Tuning

```json
"StockUpdateOptions": {
  "UpdateIntervalInSeconds": 1,
  "MaxPercentageChange": 0.002
}
```

To make movement more or less dramatic, change the per-ticker μ and σ, or raise `MaxPercentageChange`.
