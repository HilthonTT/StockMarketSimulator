using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.GetPriceHistory;

/// <param name="Range">1D, 1W, 1M or 3M.</param>
public sealed record GetPriceHistoryQuery(string Ticker, string Range) : IQuery<PriceHistoryResponse>;
