using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.GetMarketNews;

public sealed record GetMarketNewsQuery(int Limit) : IQuery<IReadOnlyList<MarketNewsResponse>>;
