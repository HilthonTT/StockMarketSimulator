using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Sell;

internal sealed record SellTransactionCommand(string Ticker, int Quantity) : ICommand<Guid>;
