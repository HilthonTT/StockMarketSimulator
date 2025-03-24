using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;

namespace StockMarketSimulator.Api.Modules.Stocks.Application.GetByTicker;

public sealed record GetStockByTickerQuery(string Ticker) : IQuery<StockPriceResponse>;
