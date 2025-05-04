using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.GetTopPerfomer;

public sealed record GetTopPerformerQuery(Guid UserId) : IQuery<StockPriceResponse>;
