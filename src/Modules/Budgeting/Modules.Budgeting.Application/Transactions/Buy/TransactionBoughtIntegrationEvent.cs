using Application.Abstractions.Events;

namespace Modules.Budgeting.Application.Transactions.Buy;

public sealed record TransactionBoughtIntegrationEvent(Guid Id, Guid TransactionId) : IIntegrationEvent;
