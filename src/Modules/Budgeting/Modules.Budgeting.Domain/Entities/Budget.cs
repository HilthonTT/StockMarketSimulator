using Modules.Budgeting.Domain.DomainEvents;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Domain.Entities;

public sealed class Budget : Entity, IAuditable
{
    private const decimal InitialBudgetAmount = 5000m;

    private Budget(Guid id, Guid userId, decimal buyingPower)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.GreaterThanOrEqualToZero(buyingPower, nameof(buyingPower));

        Id = id;
        UserId = userId;
        BuyingPower = buyingPower;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Budget"/>
    /// </summary>
    /// <remarks>
    /// Required for EF Core
    /// </remarks>
    private Budget()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public decimal BuyingPower { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static Budget Create(Guid userId)
    {
        var budget = new Budget(Guid.CreateVersion7(), userId, InitialBudgetAmount);

        budget.Raise(new BudgetCreatedDomainEvent(Guid.CreateVersion7(), budget.Id));

        return budget;
    }

    public Result DecreaseBuyingPower(decimal amount)
    {
        if (amount < 0)
        {
            return Result.Failure(BudgetErrors.NegativeAmountNotAllowed);
        }

        if (BuyingPower < amount)
        {
            return Result.Failure(BudgetErrors.NegativeAmountNotAllowed);
        }

        BuyingPower -= amount;
        Raise(new BudgetUpdatedDomainEvent(Guid.CreateVersion7(), Id, BuyingPower));

        return Result.Success();
    }

    public Result IncreaseBuyingPower(decimal amount)
    {
        if (amount < 0)
        {
            return Result.Failure(BudgetErrors.NegativeAmountNotAllowed);
        }

        BuyingPower += amount;
        Raise(new BudgetUpdatedDomainEvent(Guid.CreateVersion7(), Id, BuyingPower));

        return Result.Success();
    }
}
