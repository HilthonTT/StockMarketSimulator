using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Orders.GetTriggered;

/// <summary>Pending orders whose trigger condition the current market price meets (oldest first).</summary>
public sealed record GetTriggeredOrderIdsQuery(int BatchSize) : IQuery<List<Guid>>;
