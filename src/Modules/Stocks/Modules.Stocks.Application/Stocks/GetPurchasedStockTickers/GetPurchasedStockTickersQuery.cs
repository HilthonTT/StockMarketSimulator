using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.GetPurchasedStockTickers;

public sealed record GetPurchasedStockTickersQuery(Guid UserId) : IQuery<PurchasedStockTickersResponse>;
