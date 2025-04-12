using Application.Abstractions.Events;

namespace Modules.Budgeting.Application.Transactions.Sell;

public sealed record TransactionSoldIntegrationEvent(Guid Id, Guid TransactionId) : IIntegrationEvent;
