using Modules.Budgeting.Domain.DomainEvents;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Budgeting.Domain.Entities;

public sealed class Budget : Entity, IAuditable
{
    private const decimal InitialBudgetAmount = 5000m;

    private Budget(Guid id, Guid userId, Money money)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));

        Ensure.NotNull(money, nameof(money));
        Ensure.GreaterThanOrEqualToZero(money.Amount, nameof(money.Amount));

        Id = id;
        UserId = userId;
        Money = money;
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

    public Money Money { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static Budget Create(Guid userId, Currency currency)
    {
        var budget = new Budget(Guid.CreateVersion7(), userId, new Money(InitialBudgetAmount, currency));

        budget.Raise(new BudgetCreatedDomainEvent(Guid.CreateVersion7(), budget.Id));

        return budget;
    }

    public Result DecreaseBuyingPower(Money moneyToDecrease)
    {
        if (moneyToDecrease.Amount < 0)
        {
            return Result.Failure(BudgetErrors.NegativeAmountNotAllowed);
        }

        if (Money.Amount < moneyToDecrease.Amount)
        {
            return Result.Failure(BudgetErrors.NegativeAmountNotAllowed);
        }

        Money -= moneyToDecrease;
        Raise(new BudgetUpdatedDomainEvent(Guid.CreateVersion7(), Id, Money.Amount));

        return Result.Success();
    }

    public Result IncreaseBuyingPower(Money moneyToIncrease)
    {
        if (moneyToIncrease.Amount < 0)
        {
            return Result.Failure(BudgetErrors.NegativeAmountNotAllowed);
        }

        Money += moneyToIncrease;
        Raise(new BudgetUpdatedDomainEvent(Guid.CreateVersion7(), Id, Money.Amount));

        return Result.Success();
    }
}
