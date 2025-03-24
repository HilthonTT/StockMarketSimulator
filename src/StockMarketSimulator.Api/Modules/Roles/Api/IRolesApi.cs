using Npgsql;

namespace StockMarketSimulator.Api.Modules.Roles.Api;

public interface IRolesApi
{
    Task CreateUserRoleAsync(
        NpgsqlConnection connection,
        UserRoleApiResponse userRoleApiResponse,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetUserRoleNamesByUserIdAsync(
        NpgsqlConnection connection, 
        Guid userId,
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default);
}
