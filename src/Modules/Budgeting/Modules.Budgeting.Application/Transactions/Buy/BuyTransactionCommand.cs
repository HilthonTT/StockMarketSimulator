using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Transactions.Buy;

public sealed record BuyTransactionCommand(Guid UserId, string Ticker, int Quantity) : ICommand<Guid>;
