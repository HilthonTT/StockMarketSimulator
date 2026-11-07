using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Domain.UnitTests.Budgeting;

public sealed class PositionTests
{
    [Fact]
    public void Open_Should_NormalizeTicker_AndStartEmpty()
    {
        var position = Position.Open(Guid.CreateVersion7(), " aapl ");

        Assert.Equal("AAPL", position.Ticker);
        Assert.Equal(0, position.Quantity);
        Assert.Equal(0m, position.TotalCost);
        Assert.Equal(0m, position.AverageCost);
    }

    [Fact]
    public void Buy_Should_AccumulateQuantityAndCost_WithWeightedAverage()
    {
        var position = Position.Open(Guid.CreateVersion7(), "AAPL");

        position.Buy(3, 100m);
        position.Buy(1, 120m);

        Assert.Equal(4, position.Quantity);
        Assert.Equal(420m, position.TotalCost);
        Assert.Equal(105m, position.AverageCost);
    }

    [Fact]
    public void Sell_Should_KeepAverageCost_AndReduceCostBasis()
    {
        var position = Position.Open(Guid.CreateVersion7(), "AAPL");
        position.Buy(4, 105m);

        Result result = position.Sell(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, position.Quantity);
        Assert.Equal(315m, position.TotalCost);
        Assert.Equal(105m, position.AverageCost);
    }

    [Fact]
    public void Sell_Should_ResetCost_WhenPositionIsClosed()
    {
        var position = Position.Open(Guid.CreateVersion7(), "AAPL");
        position.Buy(3, 33.33m);

        position.Sell(3);

        Assert.Equal(0, position.Quantity);
        Assert.Equal(0m, position.TotalCost);
        Assert.Equal(0m, position.AverageCost);
    }

    [Fact]
    public void Sell_Should_Fail_WhenSellingMoreThanHeld()
    {
        var position = Position.Open(Guid.CreateVersion7(), "AAPL");
        position.Buy(2, 10m);

        Result result = position.Sell(3);

        Assert.True(result.IsFailure);
        Assert.Equal(TransactionErrors.InsufficientStock, result.Error);
        Assert.Equal(2, position.Quantity);
        Assert.Equal(20m, position.TotalCost);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BuyAndSell_Should_RejectNonPositiveQuantities(int quantity)
    {
        var position = Position.Open(Guid.CreateVersion7(), "AAPL");

        Assert.True(position.Buy(quantity, 10m).IsFailure);
        Assert.True(position.Sell(quantity).IsFailure);
    }

    [Fact]
    public void RebuyingAfterClosing_Should_StartANewCostBasis()
    {
        var position = Position.Open(Guid.CreateVersion7(), "AAPL");
        position.Buy(2, 50m);
        position.Sell(2);

        position.Buy(1, 80m);

        Assert.Equal(1, position.Quantity);
        Assert.Equal(80m, position.AverageCost);
    }
}
