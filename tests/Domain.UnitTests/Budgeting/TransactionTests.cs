using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Domain.UnitTests.Budgeting;

public sealed class TransactionTests
{
    // Budgets start with 5,000 USD.
    private static Budget CreateBudget() => Budget.Create(Guid.CreateVersion7(), Currency.Usd);

    [Fact]
    public void Create_Buy_ShouldDebitPriceTimesQuantity()
    {
        Budget budget = CreateBudget();

        Result<Transaction> result = Transaction.Create(
            budget, "AAPL", new Money(100m, Currency.Usd), TransactionType.Expense, quantity: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(4_000m, budget.Money.Amount);
    }

    [Fact]
    public void Create_Sell_ShouldCreditPriceTimesQuantity()
    {
        Budget budget = CreateBudget();

        Result<Transaction> result = Transaction.Create(
            budget, "AAPL", new Money(100m, Currency.Usd), TransactionType.Income, quantity: 3);

        Assert.True(result.IsSuccess);
        Assert.Equal(5_300m, budget.Money.Amount);
    }

    [Fact]
    public void Create_Buy_ShouldFailWithInsufficientBuyingPower_WhenTotalExceedsBudget()
    {
        Budget budget = CreateBudget();

        // One share (100) is affordable, 1,000 shares (100,000) are not.
        Result<Transaction> result = Transaction.Create(
            budget, "AAPL", new Money(100m, Currency.Usd), TransactionType.Expense, quantity: 1_000);

        Assert.True(result.IsFailure);
        Assert.Equal(BudgetErrors.InsufficientBuyingPower, result.Error);
        Assert.Equal(5_000m, budget.Money.Amount);
    }

    [Fact]
    public void Create_Buy_ShouldAllowSpendingTheEntireBudget()
    {
        Budget budget = CreateBudget();

        Result<Transaction> result = Transaction.Create(
            budget, "AAPL", new Money(500m, Currency.Usd), TransactionType.Expense, quantity: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, budget.Money.Amount);
    }
}
