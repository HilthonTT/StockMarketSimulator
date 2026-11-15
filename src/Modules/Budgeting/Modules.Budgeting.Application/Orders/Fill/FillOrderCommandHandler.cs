using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Application.Trading;
using Modules.Budgeting.Application.Transactions;
using Modules.Budgeting.Application.Transactions.Buy;
using Modules.Budgeting.Application.Transactions.Sell;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Stocks.Api.Api;
using Modules.Stocks.Api.Responses;
using SharedKernel;

namespace Modules.Budgeting.Application.Orders.Fill;

/// <summary>
/// Runs from the order job, without a user context: the order itself says whose money moves.
/// </summary>
internal sealed class FillOrderCommandHandler(
    IOrderRepository orderRepository,
    TradeExecutor tradeExecutor,
    IStocksApi stocksApi,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<FillOrderCommand, bool>
{
    public async Task<Result<bool>> Handle(FillOrderCommand request, CancellationToken cancellationToken)
    {
        Option<Order> optionOrder = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (!optionOrder.IsSome)
        {
            return Result.Failure<bool>(OrderErrors.NotFound(request.OrderId));
        }

        Order order = optionOrder.ValueOrThrow();

        Option<StockApiResponse> quote = await stocksApi.GetByTickerAsync(order.Ticker, cancellationToken);
        if (!quote.IsSome || !order.IsTriggeredBy(quote.ValueOrThrow().Price))
        {
            // The price moved back before we got here; try again on a later tick.
            return false;
        }

        decimal price = quote.ValueOrThrow().Price;

        Result<Transaction> trade = order.Side == OrderSide.Buy
            ? await tradeExecutor.BuyAsync(order.UserId, order.Ticker, order.Quantity, price, cancellationToken)
            : await tradeExecutor.SellAsync(order.UserId, order.Ticker, order.Quantity, price, cancellationToken);

        if (trade.IsFailure)
        {
            // Not enough cash or shares any more: the order can't ever fill as placed.
            order.Reject(trade.Error.Description);
        }
        else
        {
            order.MarkFilled(price, trade.Value.Id, dateTimeProvider.UtcNow);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is DbUpdateConcurrencyException or UniqueConstraintException)
        {
            // A concurrent trade or cancel won; the order stays as it is and is re-evaluated next tick.
            return Result.Failure<bool>(BudgetErrors.ConcurrentModification);
        }

        if (trade.IsSuccess)
        {
            Task invalidate = order.Side == OrderSide.Buy
                ? TransactionCacheInvalidator.InvalidateAsync(
                    cacheService, new TransactionBoughtCacheInvalidateEvent(trade.Value.Id, order.UserId), cancellationToken)
                : TransactionCacheInvalidator.InvalidateAsync(
                    cacheService, new TransactionSoldCacheInvalidateEvent(trade.Value.Id, order.UserId), cancellationToken);

            await invalidate;
        }

        return true;
    }
}
