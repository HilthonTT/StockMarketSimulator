using Npgsql;

namespace StockMarketSimulator.Api.Modules.Users.Domain;

internal interface IUserRepository
{
    Task CreateAsync(
        NpgsqlConnection connection,
        User user, 
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null);

    Task UpdateAsync(
        NpgsqlConnection connection,
        User user,
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null);

    Task<User?> GetByIdAsync(
        NpgsqlConnection connection,
        Guid id, 
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null);

    Task<User?> GetByEmailAsync(
        NpgsqlConnection connection,
        string email, 
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null);

    Task<bool> ExistsByEmailAsync(
        NpgsqlConnection connection, 
        string email, 
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null);

    Task DeleteAllAsync(
        NpgsqlConnection connection, 
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}
