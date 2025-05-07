using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Budgeting.Domain.Enums;

public abstract class TransactionTypeV2 : Enumeration<TransactionTypeV2>
{
    public static readonly TransactionTypeV2 Expense = new ExpenseTransactionType();

    public static readonly TransactionTypeV2 Income = new IncomeTransactionType();

    protected TransactionTypeV2(int id, string name)
        : base(id, name)
    {
    }

    /// <summary>
    /// Validates that the specified monetary amount is valid for the current transaction type.
    /// </summary>
    /// <param name="money">The monetary amount to validate.</param>
    /// <returns>The success result if the validation was successful, otherwise an error result.</returns>
    public abstract Result ValidateAmount(Money money);

    private sealed class ExpenseTransactionType : TransactionTypeV2
    {
        public ExpenseTransactionType()
            : base(1, nameof(Expense))
        {
        }

        public override Result ValidateAmount(Money money)
        {
            if (money.Amount < decimal.Zero)
            {
                return Result.Success();
            }

            return Result.Failure(TransactionErrors.ExpenseAmountGreaterThanOrEqualToZero);
        }
    }

    private sealed class IncomeTransactionType : TransactionTypeV2
    {
        public IncomeTransactionType()
            : base(2, nameof(Income))
        {
        }

        public override Result ValidateAmount(Money money)
        {
            if (money.Amount > decimal.Zero)
            {
                return Result.Success();
            }

            return Result.Failure(TransactionErrors.IncomeAmountLessThanOrEqualToZero);
        }
    }
}
