using Dapper;
using Npgsql;
using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Persistence;

internal sealed class UserRepository : IUserRepository
{
    public async Task CreateAsync(
        NpgsqlConnection connection,
        User user, 
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null)
    {
        const string sql =
          """
            INSERT INTO public.users (id, email, user_name, password_hash)
            VALUES (@Id, @Email, @Username, @PasswordHash);
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
            },
            transaction: transaction);
    }

    public Task<User?> GetByEmailAsync(
        NpgsqlConnection connection,
        string email,
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null)
    {
        const string sql =
            """
            SELECT 
                id AS Id,
                email AS Email,
                user_name AS Username,
                password_hash AS PasswordHash
            FROM public.users
            WHERE email = @Email;
            """;

        return connection.QueryFirstOrDefaultAsync<User>(
            sql, 
            new { Email = email },
            transaction: transaction);
    }

    public Task<User?> GetByIdAsync(
        NpgsqlConnection connection,
        Guid id, 
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null)
    {
        const string sql =
            """
            SELECT 
                id AS Id,
                email AS Email,
                user_name AS Username,
                password_hash AS PasswordHash
            FROM public.users
            WHERE id = @Id;
            """;

        return connection.QueryFirstOrDefaultAsync<User>(
            sql,
            new { Id = id }, 
            transaction: transaction);
    }

    public async Task UpdateAsync(
        NpgsqlConnection connection,
        User user, 
        CancellationToken cancellationToken = default, 
        NpgsqlTransaction? transaction = null)
    {
        const string sql =
            """
            UPDATE public.users
            SET email = @Email,
                user_name = @Username,
                password_hash = @PasswordHash
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
            },
            transaction: transaction);
    }

    public Task<bool> ExistsByEmailAsync(
        NpgsqlConnection connection,
        string email,
        CancellationToken cancellationToken = default,
        NpgsqlTransaction? transaction = null)
    {
        const string sql =
            """
            SELECT EXISTS(
                SELECT 1 FROM public.users WHERE email = @Email
            );
            """;

        return connection.ExecuteScalarAsync<bool>(
            sql,
            new { Email = email },
            transaction: transaction);
    }

    public Task DeleteAllAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            DELETE 
            FROM public.users
            """;

        return connection.ExecuteAsync(sql, transaction: transaction);
    }
}
