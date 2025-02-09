using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Budgets.Domain;

internal static class BudgetErrors
{
    public static Error NotFoundByUserId(Guid userId) => Error.NotFound(
        "Budgets.NotFoundByUserId",
        $"The budget with user identifier = '{userId}' was not found");
}
