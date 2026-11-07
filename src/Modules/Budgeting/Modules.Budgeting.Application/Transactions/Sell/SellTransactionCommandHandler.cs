using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Domain.ValueObjects;
using Modules.Stocks.Api.Api;
using Modules.Stocks.Api.Responses;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed class SellTransactionCommandHandler(
    IBudgetRepository budgetRepository,
    ITransactionRepository transactionRepository,
    IPositionRepository positionRepository,
    IStocksApi stocksApi,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : ICommandHandler<SellTransactionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SellTransactionCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized);
        }

        Option<Budget> optionBudget = await budgetRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (!optionBudget.IsSome)
        {
            return Result.Failure<Guid>(BudgetErrors.NotFoundByUserId(request.UserId));
        }

        Budget budget = optionBudget.ValueOrThrow();

        // Tickers are upper-case everywhere (stocks, positions, transactions).
        string ticker = Position.NormalizeTicker(request.Ticker);

        Option<StockApiResponse> optionStockInfo = await stocksApi.GetByTickerAsync(ticker, cancellationToken);
        if (!optionStockInfo.IsSome)
        {
            return Result.Failure<Guid>(StockErrors.NotFound(ticker));
        }

        StockApiResponse stockInfo = optionStockInfo.ValueOrThrow();

        // The position row is the source of truth for holdings (no replay of the transaction history).
        Option<Position> optionPosition = await positionRepository.GetAsync(request.UserId, ticker, cancellationToken);
        if (!optionPosition.IsSome)
        {
            return Result.Failure<Guid>(TransactionErrors.InsufficientStock);
        }

        Position position = optionPosition.ValueOrThrow();

        Result positionResult = position.Sell(request.Quantity);
        if (positionResult.IsFailure)
        {
            return Result.Failure<Guid>(positionResult.Error);
        }

        var money = new Money(stockInfo.Price, Currency.Usd);

        Result<Transaction> transactionResult =
            Transaction.Create(budget, ticker, money, TransactionType.Income, request.Quantity);

        if (transactionResult.IsFailure)
        {
            return Result.Failure<Guid>(transactionResult.Error);
        }

        Transaction transaction = transactionResult.Value;

        transactionRepository.Insert(transaction);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is DbUpdateConcurrencyException or UniqueConstraintException)
        {
            // Another trade for the same user committed first: either the budget's xmin row version
            // changed, or a concurrent first buy of this ticker already created the position row.
            return Result.Failure<Guid>(BudgetErrors.ConcurrentModification);
        }

        await TransactionCacheInvalidator.InvalidateAsync(
            cacheService,
            new TransactionSoldCacheInvalidateEvent(transaction.Id, transaction.UserId),
            cancellationToken);

        return transaction.Id;
    }
}
