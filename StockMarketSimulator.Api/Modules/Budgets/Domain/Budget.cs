namespace StockMarketSimulator.Api.Modules.Budgets.Domain;

internal sealed class Budget
{
    public required Guid Id { get; set; }

    public required Guid UserId { get; set; }

    public required decimal BuyingPower { get; set; }
}
