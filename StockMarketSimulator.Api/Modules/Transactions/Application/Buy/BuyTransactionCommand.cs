using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Buy;

internal sealed record BuyTransactionCommand(
    string Ticker,
    decimal LimitPrice,
    int Quantity) : ICommand<Guid>;
