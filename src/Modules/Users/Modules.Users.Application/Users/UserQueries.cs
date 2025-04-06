using System.Data;
using Dapper;
using Modules.Users.Contracts.Users;
using SharedKernel;

namespace Modules.Users.Application.Users;

internal static class UserQueries
{
    public async static Task<Option<UserResponse>> GetByIdAsync(
        IDbConnection connection,
        Guid userId)
    {
        const string sql =
            """
            SELECT 
                u.id AS Id,
                u.email AS Email,
                u.username AS Username,
                u.created_on_utc AS CreatedOnUtc,
                u.modified_on_utc AS ModifiedOnUtc
            FROM users.users u
            WHERE u.id = @UserId
            LIMIT 1;
            """;

        UserResponse? user = await connection.QueryFirstOrDefaultAsync<UserResponse>(sql, new { UserId = userId });

        return Option<UserResponse>.Some(user);
    }
}
