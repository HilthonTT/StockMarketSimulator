using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Domain.Entities;

/// <summary>
/// A conditional order that waits for the market: a limit or a stop order on one ticker.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>Limit buy: fills when the price drops to the trigger or below.</item>
///   <item>Limit sell: fills when the price rises to the trigger or above.</item>
///   <item>Stop buy: fills when the price rises to the trigger or above (breakout).</item>
///   <item>Stop sell (stop-loss): fills when the price drops to the trigger or below.</item>
/// </list>
/// A triggered order executes as a market order at the price at that moment. Cash and shares are not
/// reserved while the order waits; they're checked when it fills, and the order is rejected if they fall short.
/// </remarks>
public sealed class Order : Entity, IAuditable
{
    private Order(Guid id, Guid userId, string ticker, OrderSide side, OrderType type, int quantity, decimal triggerPrice)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));

        Id = id;
        UserId = userId;
        Ticker = ticker;
        Side = side;
        Type = type;
        Quantity = quantity;
        TriggerPrice = triggerPrice;
        Status = OrderStatus.Pending;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/>
    /// </summary>
    /// <remarks>
    /// Required for EF Core
    /// </remarks>
    private Order()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Ticker { get; private set; } = null!;

    public OrderSide Side { get; private set; }

    public OrderType Type { get; private set; }

    public int Quantity { get; private set; }

    public decimal TriggerPrice { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal? FilledPrice { get; private set; }

    public DateTime? FilledOnUtc { get; private set; }

    public Guid? TransactionId { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public bool IsPending => Status == OrderStatus.Pending;

    public static Result<Order> Place(
        Guid userId,
        string ticker,
        OrderSide side,
        OrderType type,
        int quantity,
        decimal triggerPrice)
    {
        if (quantity <= 0)
        {
            return Result.Failure<Order>(TransactionErrors.NegativeQuantityNotAllowed);
        }

        if (triggerPrice <= 0)
        {
            return Result.Failure<Order>(OrderErrors.TriggerPriceMustBePositive);
        }

        return new Order(
            Guid.CreateVersion7(),
            userId,
            Position.NormalizeTicker(ticker),
            side,
            type,
            quantity,
            triggerPrice);
    }

    /// <summary>Whether a market price meets this order's trigger condition.</summary>
    public bool IsTriggeredBy(decimal marketPrice)
    {
        if (!IsPending || marketPrice <= 0)
        {
            return false;
        }

        bool fillsOnDrop = (Side, Type) is (OrderSide.Buy, OrderType.Limit) or (OrderSide.Sell, OrderType.Stop);

        return fillsOnDrop ? marketPrice <= TriggerPrice : marketPrice >= TriggerPrice;
    }

    public Result MarkFilled(decimal price, Guid transactionId, DateTime utcNow)
    {
        if (!IsPending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Filled;
        FilledPrice = price;
        TransactionId = transactionId;
        FilledOnUtc = utcNow;

        return Result.Success();
    }

    public Result Reject(string reason)
    {
        if (!IsPending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Rejected;
        RejectionReason = reason.Length > 300 ? reason[..300] : reason;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (!IsPending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Cancelled;

        return Result.Success();
    }
}
