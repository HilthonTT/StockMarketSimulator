using Application.Abstractions.Messaging;
using Modules.Budgeting.Contracts.Orders;

namespace Modules.Budgeting.Application.Orders.GetByUserId;

/// <param name="OpenOnly">Only pending orders.</param>
public sealed record GetOrdersByUserIdQuery(Guid UserId, bool OpenOnly) : IQuery<List<OrderResponse>>;
