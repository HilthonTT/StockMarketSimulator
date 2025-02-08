using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Transactions.Domain;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Create;

internal sealed record CreateTransactionCommand(
    string Ticker, 
    decimal LimitPrice, 
    TransactionType Type, 
    int Quantity) : ICommand<Guid>;
