using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.GetByTicker;

public sealed record GetStockByTickerQuery(string Ticker) : IQuery<StockPriceResponse>;
