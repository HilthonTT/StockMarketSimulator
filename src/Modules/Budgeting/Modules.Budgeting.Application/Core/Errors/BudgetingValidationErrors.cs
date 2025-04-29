using SharedKernel;

namespace Modules.Budgeting.Application.Core.Errors;

internal static class BudgetingValidationErrors
{
    public static class GetBudgetByUserId
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "GetBudgetByUserId.UserIdIsRequired",
            "The user identifier is required.");
    }

    public static class BuyTransaction
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "BuyTransaction.UserIdIsRequired",
            "The user identifier is required.");

        public static readonly Error TickerIsRequired = Error.Problem(
            "BuyTransaction.TickerIsRequired",
            "The ticker SYMBOL is required.");

        public static readonly Error TickerInvalidFormat = Error.Problem(
            "BuyTransaction.TickerInvalidFormat",
            "The ticker must be at most 10 characters.");

        public static readonly Error QuantityMustBeAtleastOne = Error.Problem(
            "BuyTransaction.QuantityMustBeAtleastOne",
            "The quantity must at least be 1.");
    }

    public static class GetTransactionsByUserId
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
           "GetTransactionsByUserId.UserIdIsRequired",
           "The user identifier is required.");
    }

    public static class SellTransaction
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "SellTransaction.UserIdIsRequired",
            "The user identifier is required.");

        public static readonly Error TickerIsRequired = Error.Problem(
            "SellTransaction.TickerIsRequired",
            "The ticker SYMBOL is required.");

        public static readonly Error TickerInvalidFormat = Error.Problem(
            "SellTransaction.TickerInvalidFormat",
            "The ticker must be at most 10 characters.");

        public static readonly Error QuantityMustBeAtleastOne = Error.Problem(
            "SellTransaction.QuantityMustBeAtleastOne",
            "The quantity must at least be 1.");
    }
}
