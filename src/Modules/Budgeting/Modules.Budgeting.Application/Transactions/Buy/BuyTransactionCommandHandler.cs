using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Stocks.Api;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.Buy;

internal sealed class BuyTransactionCommandHandler(
    IBudgetRepository budgetRepository,
    ITransactionRepository transactionRepository,
    IStocksApi stocksApi,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<BuyTransactionCommand, Guid>
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

        Option<StockApiResponse> optionStockInfo = await stocksApi.GetByTickerAsync(request.Ticker, cancellationToken);
        if (!optionStockInfo.IsSome)
        {
            return Result.Failure<Guid>(StockErrors.NotFound(request.Ticker));
        }

        StockApiResponse stockInfo = optionStockInfo.ValueOrThrow();

        Result<Transaction> transactionResult =
            Transaction.Create(budget, request.Ticker, stockInfo.Price, TransactionType.Buy, request.Quantity);

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
