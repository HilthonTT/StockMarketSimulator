using SharedKernel;

namespace Modules.Budgeting.Domain.Errors;

public static class OrderErrors
{
    public const int MaxOpenOrders = 50;

    public static readonly Error TriggerPriceMustBePositive = Error.Problem(
        "Orders.TriggerPriceMustBePositive",
        "The trigger price must be greater than zero.");

    public static readonly Error NotPending = Error.Conflict(
        "Orders.NotPending",
        "Only pending orders can be changed.");

    public static readonly Error TooManyOpenOrders = Error.Problem(
        "Orders.TooManyOpenOrders",
        $"You can have at most {MaxOpenOrders} open orders. Cancel one first.");

    public static Error NotFound(Guid orderId) => Error.NotFound(
        "Orders.NotFound",
        $"The order with identifier = '{orderId}' was not found.");
}
