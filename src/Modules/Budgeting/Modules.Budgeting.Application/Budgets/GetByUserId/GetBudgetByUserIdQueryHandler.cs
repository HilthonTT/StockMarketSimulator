using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Contracts.Budgets;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Application.Budgets.GetByUserId;

internal sealed class GetBudgetByUserIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext) : IQueryHandler<GetBudgetByUserIdQuery, BudgetResponse>
{
    public async Task<Result<BudgetResponse>> Handle(GetBudgetByUserIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<BudgetResponse>(UserErrors.Unauthorized);
        }

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        Option<BudgetResponse> optionBudget = 
            await BudgetQueries.GetByUserIdAsync(connection, request.UserId, cancellationToken);
        if (!optionBudget.IsSome)
        {
            return Result.Failure<BudgetResponse>(BudgetErrors.NotFoundByUserId(request.UserId));
        }

        BudgetResponse budget = optionBudget.ValueOrThrow();

        return budget;
    }
}
