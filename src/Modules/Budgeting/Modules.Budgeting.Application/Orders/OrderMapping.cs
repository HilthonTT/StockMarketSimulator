using Modules.Budgeting.Domain.Enums;

namespace Modules.Budgeting.Application.Orders;

internal static class OrderMapping
{
    public static bool TryParseSide(string? value, out OrderSide side)
    {
        side = default;
        return value?.Trim().ToLowerInvariant() switch
        {
            "buy" => Set(out side, OrderSide.Buy),
            "sell" => Set(out side, OrderSide.Sell),
            _ => false,
        };
    }

    public static bool TryParseType(string? value, out OrderType type)
    {
        type = default;
        return value?.Trim().ToLowerInvariant() switch
        {
            "limit" => Set(out type, OrderType.Limit),
            "stop" => Set(out type, OrderType.Stop),
            _ => false,
        };
    }

    public static string ToApi(this OrderSide side) => side == OrderSide.Buy ? "buy" : "sell";

    public static string ToApi(this OrderType type) => type == OrderType.Limit ? "limit" : "stop";

    public static string ToApi(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "pending",
        OrderStatus.Filled => "filled",
        OrderStatus.Cancelled => "cancelled",
        _ => "rejected",
    };

    private static bool Set<T>(out T target, T value)
    {
        target = value;
        return true;
    }
}
