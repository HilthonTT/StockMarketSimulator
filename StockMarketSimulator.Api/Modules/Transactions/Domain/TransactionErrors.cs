using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Transactions.Domain;

internal static class TransactionErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Transactions.NotFound",
        $"The transaction with the Id = '{id}' was not found");
}
