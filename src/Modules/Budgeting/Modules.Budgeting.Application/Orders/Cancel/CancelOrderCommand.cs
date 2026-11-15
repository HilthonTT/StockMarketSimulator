using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand;
