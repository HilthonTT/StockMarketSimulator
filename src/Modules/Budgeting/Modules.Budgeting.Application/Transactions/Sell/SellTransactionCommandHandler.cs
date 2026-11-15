using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Application.Trading;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using Modules.Stocks.Api.Api;
using Modules.Stocks.Api.Responses;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed class SellTransactionCommandHandler(
    TradeExecutor tradeExecutor,
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

        // Tickers are upper-case everywhere (stocks, positions, transactions).
        string ticker = Position.NormalizeTicker(request.Ticker);

        Option<StockApiResponse> optionStockInfo = await stocksApi.GetByTickerAsync(ticker, cancellationToken);
        if (!optionStockInfo.IsSome)
        {
            return Result.Failure<Guid>(StockErrors.NotFound(ticker));
        }

        Result<Transaction> result = await tradeExecutor.SellAsync(
            request.UserId,
            ticker,
            request.Quantity,
            optionStockInfo.ValueOrThrow().Price,
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

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
            new TransactionSoldCacheInvalidateEvent(result.Value.Id, result.Value.UserId),
            cancellationToken);

        return result.Value.Id;
    }
}
