using Dapper;
using Npgsql;
using StockMarketSimulator.Api.Modules.Roles.Domain;

namespace StockMarketSimulator.Api.Modules.Roles.Persistence;

internal sealed class UserRoleRepository : IUserRoleRepository
{
    public Task CreateAsync(
        NpgsqlConnection connection, 
        UserRole userRole, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO public.user_roles (user_id, role_id)
            VALUES (@UserId, @RoleId)
            ON CONFLICT (user_id, role_id) DO NOTHING;
            """;

        return connection.ExecuteAsync(
            sql,
            new { userRole.UserId, userRole.RoleId },
            transaction: transaction);
    }

    public async Task<List<string>> GetRoleNamesByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT r.name 
            FROM public.user_roles ur
            INNER JOIN public.roles r ON ur.role_id = r.id
            WHERE ur.user_id = @UserId;
            """;

        IEnumerable<string> roles = await connection.QueryAsync<string>(
            sql,
            new { UserId = userId },
            transaction: transaction);

        return [.. roles];
    }
}
