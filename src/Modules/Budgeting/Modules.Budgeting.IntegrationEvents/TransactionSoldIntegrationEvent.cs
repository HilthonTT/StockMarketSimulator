using Application.Abstractions.Events;

namespace Modules.Budgeting.IntegrationEvents;

/// <summary>Published when a sell transaction is committed.</summary>
public sealed record TransactionSoldIntegrationEvent(Guid Id, Guid TransactionId) : IIntegrationEvent;
