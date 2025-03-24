using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Buy;

internal sealed record BuyTransactionCommand(string Ticker, int Quantity) : ICommand<Guid>;
