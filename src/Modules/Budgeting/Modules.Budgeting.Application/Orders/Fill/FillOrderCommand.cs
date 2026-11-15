using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Orders.Fill;

/// <summary>Executes a pending order if its trigger is (still) met. Returns whether the order changed state.</summary>
public sealed record FillOrderCommand(Guid OrderId) : ICommand<bool>;
