using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Stocks.Api;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed class SellTransactionCommandHandler(
    IBudgetRepository budgetRepository,
    ITransactionRepository transactionRepository,
    IStocksApi stocksApi,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<SellTransactionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SellTransactionCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized);
        }

        Budget? budget = await budgetRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (budget is null)
        {
            return Result.Failure<Guid>(BudgetErrors.NotFoundByUserId(request.UserId));
        }

        StockApiResponse? stockInfo = await stocksApi.GetByTickerAsync(request.Ticker, cancellationToken);
        if (stockInfo is null)
        {
            return Result.Failure<Guid>(StockErrors.NotFound(request.Ticker));
        }

        int totalOwned = await transactionRepository.CalculateNetPurchasedQuantityAsync(
            request.UserId, 
            request.Ticker, 
            cancellationToken);

        if (totalOwned < request.Quantity)
        {
            return Result.Failure<Guid>(TransactionErrors.InsufficientStock);
        }

        Result<Transaction> transactionResult =
            Transaction.Create(budget, request.Ticker, stockInfo.Price, TransactionType.Sell, request.Quantity);

        if (transactionResult.IsFailure)
        {
            return Result.Failure<Guid>(transactionResult.Error);
        }

        Transaction transaction = transactionResult.Value;

        transactionRepository.Insert(transaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}
