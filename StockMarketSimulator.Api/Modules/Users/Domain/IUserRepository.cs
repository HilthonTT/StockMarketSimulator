using Npgsql;

namespace StockMarketSimulator.Api.Modules.Users.Domain;

public interface IUserRepository
{
    Task<Guid> CreateAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default);

    Task UpdateAsync(NpgsqlConnection connection, User user, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(NpgsqlConnection connection, Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(NpgsqlConnection connection, string email, CancellationToken cancellationToken = default);
}
