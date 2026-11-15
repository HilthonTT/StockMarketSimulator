using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Orders.Place;

public sealed record PlaceOrderCommand(
    Guid UserId,
    string Ticker,
    string Side,
    string Type,
    int Quantity,
    decimal TriggerPrice) : ICommand<Guid>;
