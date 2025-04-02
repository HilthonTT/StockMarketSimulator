using SharedKernel;

namespace Modules.Budgeting.Domain.Errors;

public static class BudgetErrors
{
    public static readonly Error NegativeAmountNotAllowed = Error.Problem(
       "Budget.NegativeAmountNotAllowed",
       "The amount must be greater than or equal to zero.");

    public static readonly Error InsufficientBuyingPower = Error.Problem(
        "Budget.InsufficientBuyingPower",
        "The available buying power is insufficient to complete this transaction.");
}
