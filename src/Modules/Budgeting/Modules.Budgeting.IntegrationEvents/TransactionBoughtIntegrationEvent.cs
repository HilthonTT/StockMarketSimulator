using Application.Abstractions.Events;

namespace Modules.Budgeting.IntegrationEvents;

/// <summary>Published when a buy transaction is committed.</summary>
public sealed record TransactionBoughtIntegrationEvent(Guid Id, Guid TransactionId) : IIntegrationEvent;
