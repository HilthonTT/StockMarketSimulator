namespace Modules.Budgeting.Contracts.Orders;

/// <param name="Side"><c>buy</c> or <c>sell</c>.</param>
/// <param name="Type"><c>limit</c> or <c>stop</c>.</param>
public sealed record PlaceOrderRequest(
    Guid UserId,
    string Ticker,
    string Side,
    string Type,
    int Quantity,
    decimal TriggerPrice);

/// <summary>A limit/stop order. Enum-like fields are lower-case strings.</summary>
public sealed record OrderResponse(
    Guid Id,
    string Ticker,
    string Side,
    string Type,
    int Quantity,
    decimal TriggerPrice,
    string Status,
    decimal? FilledPrice,
    string? RejectionReason,
    DateTime CreatedOnUtc,
    DateTime? FilledOnUtc);
