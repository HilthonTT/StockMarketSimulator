using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(IDbConnectionFactory dbConnectionFactory) 
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        const string sql =
            """
            SELECT 
                u.id AS Id,
                u.email AS Email,
                u.username AS Username,
                u.created_on_utc AS CreatedOnUtc,
                u.modified_on_utc AS ModifiedOnUtc
            FROM users.users u
            WHERE u.id = @Id
            LIMIT 1;
            """;

        UserResponse? user = await connection.QueryFirstOrDefaultAsync<UserResponse>(sql, new { Id = request.UserId });
        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
        }

        return user;
    }
}
