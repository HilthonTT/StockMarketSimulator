using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Modules.Budgeting.Contracts.Positions;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Application.Positions.GetByUserId;

internal sealed class GetPositionsByUserIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext) : IQueryHandler<GetPositionsByUserIdQuery, List<PositionResponse>>
{
    public async Task<Result<List<PositionResponse>>> Handle(
        GetPositionsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        // Same ownership rule as the budget: a user can only see their own holdings.
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<List<PositionResponse>>(UserErrors.Unauthorized);
        }

        const string sql =
            """
            SELECT
                ticker AS Ticker,
                quantity AS Quantity,
                total_cost AS TotalCost
            FROM budgeting.positions
            WHERE user_id = @UserId AND quantity > 0
            ORDER BY ticker;
            """;

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        IEnumerable<PositionRow> rows = await connection.QueryAsync<PositionRow>(
            new CommandDefinition(sql, new { request.UserId }, cancellationToken: cancellationToken));

        return rows
            .Select(row => new PositionResponse(
                row.Ticker,
                row.Quantity,
                Math.Round(row.TotalCost / row.Quantity, Position.CostDecimals),
                row.TotalCost))
            .ToList();
    }

    private sealed class PositionRow
    {
        public string Ticker { get; init; } = string.Empty;

        public int Quantity { get; init; }

        public decimal TotalCost { get; init; }
    }
}
