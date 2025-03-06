using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Budgets.Domain;

internal sealed class Budget : IEntity
{
    private Budget(Guid id, Guid userId, decimal buyingPower)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.GreaterThanOrEqualToZero(buyingPower, nameof(buyingPower));

        Id = id;
        UserId = userId;
        BuyingPower = buyingPower;
    }

    private Budget()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public decimal BuyingPower { get; private set; }

    public static Budget Create(Guid userId, decimal buyingPower)
    {
        return new Budget(Guid.NewGuid(), userId, buyingPower);
    }

    public static Budget Create(Guid id, Guid userId, decimal buyingPower)
    {
        return new Budget(id, userId, buyingPower);
    }

    public void UpdateBuyingPower(decimal newBuyingPower)
    {
        Ensure.GreaterThanOrEqualToZero(newBuyingPower, nameof(newBuyingPower));

        BuyingPower = newBuyingPower;
    }
}