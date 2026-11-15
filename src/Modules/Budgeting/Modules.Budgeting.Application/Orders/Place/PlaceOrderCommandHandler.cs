using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Stocks.Api.Api;
using SharedKernel;

namespace Modules.Budgeting.Application.Orders.Place;

internal sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IBudgetRepository budgetRepository,
    IPositionRepository positionRepository,
    IStocksApi stocksApi,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized);
        }

        OrderMapping.TryParseSide(request.Side, out OrderSide side);
        OrderMapping.TryParseType(request.Type, out OrderType type);

        string ticker = Position.NormalizeTicker(request.Ticker);

        if (!(await stocksApi.GetByTickerAsync(ticker, cancellationToken)).IsSome)
        {
            return Result.Failure<Guid>(StockErrors.NotFound(ticker));
        }

        if (await orderRepository.CountPendingAsync(request.UserId, cancellationToken) >= OrderErrors.MaxOpenOrders)
        {
            return Result.Failure<Guid>(OrderErrors.TooManyOpenOrders);
        }

        // Sanity checks at placement (nothing is reserved; the fill checks again).
        if (side == OrderSide.Buy)
        {
            Option<Budget> budget = await budgetRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (!budget.IsSome)
            {
                return Result.Failure<Guid>(BudgetErrors.NotFoundByUserId(request.UserId));
            }

            if (budget.ValueOrThrow().Money.Amount < request.TriggerPrice * request.Quantity)
            {
                return Result.Failure<Guid>(BudgetErrors.InsufficientBuyingPower);
            }
        }
        else
        {
            Option<Position> position = await positionRepository.GetAsync(request.UserId, ticker, cancellationToken);
            if (!position.IsSome || position.ValueOrThrow().Quantity < request.Quantity)
            {
                return Result.Failure<Guid>(TransactionErrors.InsufficientStock);
            }
        }

        Result<Order> orderResult = Order.Place(request.UserId, ticker, side, type, request.Quantity, request.TriggerPrice);
        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        orderRepository.Insert(orderResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return orderResult.Value.Id;
    }
}
