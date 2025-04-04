using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Transactions.Sell;

public sealed record SellTransactionCommand(Guid UserId, string Ticker, int Quantity) : ICommand<Guid>;
