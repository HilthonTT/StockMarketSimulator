using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Contracts.Orders;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Application.Orders.GetByUserId;

internal sealed class GetOrdersByUserIdQueryHandler(
    IDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetOrdersByUserIdQuery, List<OrderResponse>>
{
    private const int MaxOrders = 100;

    public async Task<Result<List<OrderResponse>>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<List<OrderResponse>>(UserErrors.Unauthorized);
        }

        IQueryable<Order> query = dbContext.Orders.AsNoTracking().Where(o => o.UserId == request.UserId);

        if (request.OpenOnly)
        {
            query = query.Where(o => o.Status == OrderStatus.Pending);
        }

        List<Order> orders = await query
            .OrderByDescending(o => o.CreatedOnUtc)
            .Take(MaxOrders)
            .ToListAsync(cancellationToken);

        return orders
            .Select(o => new OrderResponse(
                o.Id,
                o.Ticker,
                o.Side.ToApi(),
                o.Type.ToApi(),
                o.Quantity,
                o.TriggerPrice,
                o.Status.ToApi(),
                o.FilledPrice,
                o.RejectionReason,
                o.CreatedOnUtc,
                o.FilledOnUtc))
            .ToList();
    }
}
