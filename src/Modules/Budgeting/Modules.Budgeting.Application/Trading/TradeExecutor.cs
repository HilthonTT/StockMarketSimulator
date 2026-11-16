using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Budgeting.Application.Trading;

/// <summary>
/// Applies a trade to a user's budget and position and records the transaction. Shared by market
/// orders and by limit/stop orders when they fill. The caller owns the unit of work.
/// </summary>
internal sealed class TradeExecutor(
    IBudgetRepository budgetRepository,
    ITransactionRepository transactionRepository,
    IPositionRepository positionRepository)
{
    public async Task<Result<Transaction>> BuyAsync(
        Guid userId,
        string ticker,
        int quantity,
        decimal pricePerShare,
        CancellationToken cancellationToken)
    {
        Option<Budget> optionBudget = await budgetRepository.GetByUserIdAsync(userId, cancellationToken);
        if (!optionBudget.IsSome)
        {
            return Result.Failure<Transaction>(BudgetErrors.NotFoundByUserId(userId));
        }

        Budget budget = optionBudget.ValueOrThrow();

        Result<Transaction> transactionResult = Transaction.Create(
            budget, ticker, new Money(pricePerShare, Currency.Usd), TransactionType.Expense, quantity);

        if (transactionResult.IsFailure)
        {
            return transactionResult;
        }

        Option<Position> optionPosition = await positionRepository.GetAsync(userId, ticker, cancellationToken);

        Position position;
        if (optionPosition.IsSome)
        {
            position = optionPosition.ValueOrThrow();
        }
        else
        {
            position = Position.Open(userId, ticker);
            positionRepository.Insert(position);
        }

        Result positionResult = position.Buy(quantity, pricePerShare);
        if (positionResult.IsFailure)
        {
            return Result.Failure<Transaction>(positionResult.Error);
        }

        transactionRepository.Insert(transactionResult.Value);

        return transactionResult;
    }

    public async Task<Result<Transaction>> SellAsync(
        Guid userId,
        string ticker,
        int quantity,
        decimal pricePerShare,
        CancellationToken cancellationToken)
    {
        Option<Budget> optionBudget = await budgetRepository.GetByUserIdAsync(userId, cancellationToken);
        if (!optionBudget.IsSome)
        {
            return Result.Failure<Transaction>(BudgetErrors.NotFoundByUserId(userId));
        }

        // The position row is the source of truth for holdings (no replay of the transaction history).
        Option<Position> optionPosition = await positionRepository.GetAsync(userId, ticker, cancellationToken);
        if (!optionPosition.IsSome)
        {
            return Result.Failure<Transaction>(TransactionErrors.InsufficientStock);
        }

        Position position = optionPosition.ValueOrThrow();

        // Check everything that can fail before mutating anything: callers such as the order fill save the
        // unit of work even when the trade fails (to record the rejection), so a half-applied trade would persist.
        if (quantity <= 0)
        {
            return Result.Failure<Transaction>(TransactionErrors.NegativeQuantityNotAllowed);
        }

        if (position.Quantity < quantity)
        {
            return Result.Failure<Transaction>(TransactionErrors.InsufficientStock);
        }

        Result<Transaction> transactionResult = Transaction.Create(
            optionBudget.ValueOrThrow(), ticker, new Money(pricePerShare, Currency.Usd), TransactionType.Income, quantity);

        if (transactionResult.IsFailure)
        {
            return transactionResult;
        }

        Result positionResult = position.Sell(quantity);
        if (positionResult.IsFailure)
        {
            // Unreachable after the checks above; fail loudly rather than persist cash without the share removal.
            throw new DomainException(positionResult.Error);
        }

        transactionRepository.Insert(transactionResult.Value);

        return transactionResult;
    }
}
