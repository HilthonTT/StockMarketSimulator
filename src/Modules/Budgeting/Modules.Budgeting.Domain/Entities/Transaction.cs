using Modules.Budgeting.Domain.DomainEvents;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Domain.Entities;

public sealed class Transaction : Entity, IAuditable
{
    private Transaction(
        Guid id,
        Guid userId,
        string ticker,
        decimal limitPrice,
        TransactionType type,
        int quantity)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));
        Ensure.GreaterThanOrEqualToZero(limitPrice, nameof(limitPrice));
        Ensure.GreaterThanOrEqualToZero(quantity, nameof(quantity));

        Id = id;
        UserId = userId;
        Ticker = ticker;
        LimitPrice = limitPrice;
        Type = type;
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

    public string Ticker { get; private set; }

    public decimal LimitPrice { get; private set; }

    public TransactionType Type { get; private set; }

    public int Quantity { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public decimal TotalAmount => LimitPrice * Quantity;

    public static Result<Transaction> Create(
        Budget budget, 
        string ticker, 
        decimal limitPrice, 
        TransactionType type, 
        int quantity)
    {
        Ensure.NotNull(budget, nameof(budget));

        decimal totalCost = limitPrice * quantity;

        if (type == TransactionType.Buy)
        {
            Result result = budget.DecreaseBuyingPower(totalCost);
            if (result.IsFailure)
            {
                return Result.Failure<Transaction>(result.Error);
            }
        }
        else if (type == TransactionType.Sell)
        {
            Result result = budget.IncreaseBuyingPower(totalCost);
            if (result.IsFailure)
            {
                return Result.Failure<Transaction>(result.Error);
            }
        }

        var transaction = new Transaction(Guid.CreateVersion7(), budget.UserId, ticker, limitPrice, type, quantity);

        switch (type)
        {
            case TransactionType.Buy:
                transaction.Raise(new TransactionBoughtDomainEvent(transaction.Id));
                break;
            case TransactionType.Sell:
                transaction.Raise(new TransactionSoldDomainEvent(transaction.Id));
                break;
            default:
                break;
        }

        return transaction;
    }

    public Result UpdateLimitPrice(decimal newLimitPrice)
    {
        if (newLimitPrice < 0)
        {
            return Result.Failure(TransactionErrors.NegativeLimitPriceNotAllowed);
        }

        LimitPrice = newLimitPrice;

        Raise(new TransactionUpdatedDomainEvent(Id));

        return Result.Success();
    }

    public Result UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0)
        {
            return Result.Failure(TransactionErrors.NegativeQuantityNotAllowed);
        }

        Quantity = newQuantity;

        Raise(new TransactionUpdatedDomainEvent(Id));

        return Result.Success();
    }
}
