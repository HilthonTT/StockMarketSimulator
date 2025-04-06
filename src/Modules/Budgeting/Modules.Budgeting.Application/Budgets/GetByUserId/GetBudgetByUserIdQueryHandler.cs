using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
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

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        const string sql =
            """
            SELECT
                id AS Id,
                user_id AS UserId,
                buying_power AS BuyingPower
            FROM budgeting.budgets
            WHERE user_id = @UserId
            LIMIT 1;
            """;

        BudgetResponse? budget = await connection.QueryFirstOrDefaultAsync<BudgetResponse>(sql, new
        {
            UserId = request.UserId,
        });

        if (budget is null)
        {
            return Result.Failure<BudgetResponse>(BudgetErrors.NotFoundByUserId(request.UserId));
        }

        return budget;
    }
}
