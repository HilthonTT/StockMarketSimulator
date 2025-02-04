using Dapper;
using Npgsql;
using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Persistence;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(
        NpgsqlConnection connection, 
        string token, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT 
                id AS Id,
                token AS Token,
                user_id AS UserId,
                expires_on_utc AS ExpiresOnUtc
            FROM public.refresh_tokens
            WHERE token = @Token
            """;

        return connection.QueryFirstOrDefaultAsync<RefreshToken>(
            sql,
            new
            {
                Token = token,
            },
            transaction: transaction);
    }

    public async Task CreateAsync(
        NpgsqlConnection connection, 
        RefreshToken refreshToken, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO public.refresh_tokens (id, token, user_id, expires_on_utc)
            VALUES (@Id, @Token, @UserId, @ExpiresOnUtc);
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = refreshToken.Id,
                Token = refreshToken.Token,
                UserId = refreshToken.UserId,
                ExpiresOnUtc = refreshToken.ExpiresOnUtc
            },
            transaction: transaction);
    }

    public async Task UpdateAsync(
        NpgsqlConnection connection,
        RefreshToken refreshToken,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            UPDATE public.refresh_tokens
            SET token = @Token,
                expires_on_utc = @ExpiresOnUtc
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = refreshToken.Id,
                Token = refreshToken.Token,
                ExpiresOnUtc = refreshToken.ExpiresOnUtc,
            },
            transaction: transaction);
    }

    public async Task DeleteByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId, 
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            DELETE FROM public.refresh_tokens
            WHERE user_id = @UserId;
            """;

        await connection.ExecuteAsync(
            sql,
            new { UserId = userId },
            transaction: transaction);
    }
}
