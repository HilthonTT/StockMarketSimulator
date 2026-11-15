using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using SharedKernel;

namespace Modules.Budgeting.Application.Orders.Cancel;

internal sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        Option<Order> optionOrder = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        // Someone else's order is reported as missing, not forbidden, so ids can't be probed.
        if (!optionOrder.IsSome || optionOrder.ValueOrThrow().UserId != userContext.UserId)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        Result result = optionOrder.ValueOrThrow().Cancel();
        if (result.IsFailure)
        {
            return result;
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The fill job got to it first.
            return Result.Failure(OrderErrors.NotPending);
        }

        return Result.Success();
    }
}
