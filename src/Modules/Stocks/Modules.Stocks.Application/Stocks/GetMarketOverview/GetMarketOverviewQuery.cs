using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.GetMarketOverview;

public sealed record GetMarketOverviewQuery : IQuery<MarketOverviewResponse>;
