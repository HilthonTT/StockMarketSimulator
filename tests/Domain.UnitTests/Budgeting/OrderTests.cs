using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Domain.UnitTests.Budgeting;

public sealed class OrderTests
{
    private static Order Place(OrderSide side, OrderType type, decimal trigger = 100m) =>
        Order.Place(Guid.CreateVersion7(), " aapl ", side, type, 5, trigger).Value;

    [Theory]
    // Limit buy fills at or below the limit.
    [InlineData(OrderSide.Buy, OrderType.Limit, 99.99, true)]
    [InlineData(OrderSide.Buy, OrderType.Limit, 100, true)]
    [InlineData(OrderSide.Buy, OrderType.Limit, 100.01, false)]
    // Limit sell fills at or above the limit.
    [InlineData(OrderSide.Sell, OrderType.Limit, 100.01, true)]
    [InlineData(OrderSide.Sell, OrderType.Limit, 99.99, false)]
    // Stop sell (stop-loss) fills at or below the stop.
    [InlineData(OrderSide.Sell, OrderType.Stop, 99, true)]
    [InlineData(OrderSide.Sell, OrderType.Stop, 101, false)]
    // Stop buy (breakout) fills at or above the stop.
    [InlineData(OrderSide.Buy, OrderType.Stop, 101, true)]
    [InlineData(OrderSide.Buy, OrderType.Stop, 99, false)]
    public void IsTriggeredBy_Should_FollowTheOrderSemantics(OrderSide side, OrderType type, double price, bool expected)
    {
        Order order = Place(side, type);

        Assert.Equal(expected, order.IsTriggeredBy((decimal)price));
    }

    [Fact]
    public void Place_Should_NormalizeTicker_AndStartPending()
    {
        Order order = Place(OrderSide.Buy, OrderType.Limit);

        Assert.Equal("AAPL", order.Ticker);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(5, 0)]
    [InlineData(5, -1)]
    public void Place_Should_RejectInvalidQuantityOrPrice(int quantity, decimal trigger)
    {
        Result<Order> result = Order.Place(Guid.CreateVersion7(), "AAPL", OrderSide.Buy, OrderType.Limit, quantity, trigger);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MarkFilled_Should_RecordTheFill_AndStopTriggering()
    {
        Order order = Place(OrderSide.Buy, OrderType.Limit);
        var transactionId = Guid.CreateVersion7();

        Result result = order.MarkFilled(98m, transactionId, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Filled, order.Status);
        Assert.Equal(98m, order.FilledPrice);
        Assert.Equal(transactionId, order.TransactionId);
        Assert.False(order.IsTriggeredBy(50m));
    }

    [Fact]
    public void Cancel_Should_Fail_WhenTheOrderIsNoLongerPending()
    {
        Order order = Place(OrderSide.Sell, OrderType.Stop);
        order.Reject("Not enough shares");

        Result result = order.Cancel();

        Assert.Equal(OrderErrors.NotPending, result.Error);
        Assert.Equal(OrderStatus.Rejected, order.Status);
    }

    [Fact]
    public void Budget_Reset_Should_RestoreTheStartingCash()
    {
        var budget = Budget.Create(Guid.CreateVersion7(), Currency.Usd);
        budget.DecreaseMoney(new Money(4_000m, Currency.Usd));

        budget.Reset();

        Assert.Equal(Budget.InitialBudgetAmount, budget.Money.Amount);
    }
}
