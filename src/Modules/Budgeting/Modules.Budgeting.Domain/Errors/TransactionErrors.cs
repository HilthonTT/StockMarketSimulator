﻿using SharedKernel;

namespace Modules.Budgeting.Domain.Errors;

public static class TransactionErrors
{
    public static readonly Error NegativeQuantityNotAllowed = Error.Problem(
        "Transactions.NegativeAmountNotAllowed",
        "The quantity must be greater than or equal to zero.");

    public static readonly Error NegativeLimitPriceNotAllowed = Error.Problem(
        "Transactions.NegativeLimitPriceNotAllowed",
        "The limit price must be greater than or equal to zero.");

    public static readonly Error InsufficientStock = Error.Problem(
        "Transactions.InsufficientStock",
        "You do not have enough stock to complete this sale transaction");

    public static Error NotFound(Guid id) => Error.NotFound(
        "Transactions.NotFound",
        $"Transaction with the Id = '{id}' was not found");

    public static readonly Error IncomeAmountLessThanOrEqualToZero = Error.Problem(
        "Transaction.AmountLessThanOrEqualToZero",
        "The income transaction amount can not be less than or equal to zero.");

    public static readonly Error ExpenseAmountGreaterThanOrEqualToZero = Error.Problem(
        "Transaction.AmountGreaterThanOrEqualToZero",
        "The expense transaction amount can not be greater than or equal to zero.");
}
