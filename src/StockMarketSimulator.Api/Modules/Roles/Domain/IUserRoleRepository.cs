using Npgsql;

namespace StockMarketSimulator.Api.Modules.Roles.Domain;

internal interface IUserRoleRepository
{
    Task CreateAsync(
        NpgsqlConnection connection, 
        UserRole userRole, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default);

    Task<List<string>> GetRoleNamesByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId, 
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}
