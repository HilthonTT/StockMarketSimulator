using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Transactions.Domain;

internal static class TransactionErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Transactions.NotFound",
        $"The transaction with the Id = '{id}' was not found");

    public static readonly Error LimitPriceExceedsMarketPrice = Error.Problem(
        "Transactions.InvalidLimitPrice",
        "The limit price cannot exceed the current market price of the stock");

    public static readonly Error InsufficientStock = Error.Problem(
        "Transactions.InsufficientStock",
        "You do not have enough stock to complete this sale transaction");
}
