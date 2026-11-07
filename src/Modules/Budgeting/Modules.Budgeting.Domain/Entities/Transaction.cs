using Modules.Budgeting.Domain.DomainEvents;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Budgeting.Domain.Entities;

public sealed class Transaction : Entity, IAuditable
{
    private Transaction(
        Guid id,
        Guid userId,
        string ticker,
        Money money,
        TransactionType type,
        int quantity)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));
        Ensure.NotNull(money, nameof(money));
        Ensure.GreaterThanOrEqualToZero(quantity, nameof(quantity));
        Ensure.NotNull(type, nameof(type));

        Id = id;
        UserId = userId;
        Ticker = ticker;
        Type = type;
        Money = money;
        Quantity = quantity;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transaction"/>
    /// </summary>
    /// <remarks>
    /// Required for EF Core
    /// </remarks>
    private Transaction()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Ticker { get; private set; } = null!;

    public TransactionType Type { get; private set; } = null!;

    public Money Money { get; private set; } = null!;

    public int Quantity { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public decimal TotalAmount => Money.Amount * Quantity;

    public static Result<Transaction> Create(
        Budget budget, 
        string ticker, 
        Money money, 
        TransactionType type, 
        int quantity)
    {
        Ensure.NotNull(budget, nameof(budget));
        Ensure.GreaterThanZero(quantity, nameof(quantity));

        var transaction = new Transaction(Guid.CreateVersion7(), budget.UserId, ticker, money, type, quantity);

        // 'money' is the per-share price; the budget moves by price x quantity.
        var totalMoney = new Money(transaction.TotalAmount, money.Currency);

        if (type == TransactionType.Expense)
        {
            Result result = budget.DecreaseMoney(totalMoney);
            if (result.IsFailure)
            {
                return Result.Failure<Transaction>(result.Error);
            }

            transaction.Raise(new TransactionBoughtDomainEvent(Guid.CreateVersion7(), transaction.Id));
        }
        else if (type == TransactionType.Income)
        {
            Result result = budget.IncreaseMoney(totalMoney);
            if (result.IsFailure)
            {
                return Result.Failure<Transaction>(result.Error);
            }

            transaction.Raise(new TransactionSoldDomainEvent(Guid.CreateVersion7(), transaction.Id));
        }

        return transaction;
    }
}
