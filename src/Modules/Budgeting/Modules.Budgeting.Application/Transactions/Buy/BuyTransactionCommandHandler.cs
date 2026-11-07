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

namespace Modules.Budgeting.Application.Transactions.Buy;

internal sealed class BuyTransactionCommandHandler(
    IBudgetRepository budgetRepository,
    ITransactionRepository transactionRepository,
    IPositionRepository positionRepository,
    IStocksApi stocksApi,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : ICommandHandler<BuyTransactionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(BuyTransactionCommand request, CancellationToken cancellationToken)
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

        var money = new Money(stockInfo.Price, Currency.Usd);

        Result<Transaction> transactionResult =
            Transaction.Create(budget, ticker, money, TransactionType.Expense, request.Quantity);

        if (transactionResult.IsFailure)
        {
            return Result.Failure<Guid>(transactionResult.Error);
        }

        Transaction transaction = transactionResult.Value;

        Option<Position> optionPosition = await positionRepository.GetAsync(request.UserId, ticker, cancellationToken);

        Position position;
        if (optionPosition.IsSome)
        {
            position = optionPosition.ValueOrThrow();
        }
        else
        {
            position = Position.Open(request.UserId, ticker);
            positionRepository.Insert(position);
        }

        Result positionResult = position.Buy(request.Quantity, stockInfo.Price);
        if (positionResult.IsFailure)
        {
            return Result.Failure<Guid>(positionResult.Error);
        }

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
            new TransactionBoughtCacheInvalidateEvent(transaction.Id, transaction.UserId),
            cancellationToken);

        return transaction.Id;
    }
}
