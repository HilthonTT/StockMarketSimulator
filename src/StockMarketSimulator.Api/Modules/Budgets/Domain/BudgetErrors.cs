using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Budgets.Domain;

public static class BudgetErrors
{
    public static Error NotFoundByUserId(Guid userId) => Error.NotFound(
        "Budgets.NotFoundByUserId",
        $"The budget with user identifier = '{userId}' was not found");
}
