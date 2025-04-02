using SharedKernel;

namespace Modules.Budgeting.Domain.Errors;

public static class TransactionErrors
{
    public static readonly Error NegativeQuantityNotAllowed = Error.Problem(
       "Budget.NegativeAmountNotAllowed",
       "The quantity must be greater than or equal to zero.");

    public static readonly Error NegativeLimitPriceNotAllowed = Error.Problem(
       "Budget.NegativeLimitPriceNotAllowed",
       "The limit price must be greater than or equal to zero.");
}
