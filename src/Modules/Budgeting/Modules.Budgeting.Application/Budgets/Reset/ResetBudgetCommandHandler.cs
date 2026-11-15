using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using SharedKernel;

namespace Modules.Budgeting.Application.Budgets.Reset;

internal sealed class ResetBudgetCommandHandler(
    IBudgetRepository budgetRepository,
    IDbContext dbContext,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    ICacheService cacheService) : ICommandHandler<ResetBudgetCommand>
{
    public async Task<Result> Handle(ResetBudgetCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        Option<Budget> optionBudget = await budgetRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (!optionBudget.IsSome)
        {
            return Result.Failure(BudgetErrors.NotFoundByUserId(request.UserId));
        }

        optionBudget.ValueOrThrow().Reset();

        // Bulk deletes run immediately, so make them and the budget update one database transaction.
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await ((DbContext)dbContext).Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Positions.Where(p => p.UserId == request.UserId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Transactions.Where(t => t.UserId == request.UserId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Orders
            .Where(o => o.UserId == request.UserId && o.Status == OrderStatus.Pending)
            .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status, OrderStatus.Cancelled), cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(BudgetErrors.ConcurrentModification);
        }

        await transaction.CommitAsync(cancellationToken);

        await Task.WhenAll(
            cacheService.RemoveAsync($"users:{request.UserId}:purchased-stock-tickers", cancellationToken),
            cacheService.RemoveAsync($"users:{request.UserId}:transaction-count", cancellationToken));

        return Result.Success();
    }
}
