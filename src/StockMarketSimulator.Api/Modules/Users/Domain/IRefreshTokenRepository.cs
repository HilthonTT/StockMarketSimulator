using Npgsql;

namespace StockMarketSimulator.Api.Modules.Users.Domain;

internal interface IRefreshTokenRepository
{
    Task CreateAsync(
        NpgsqlConnection connection,
        RefreshToken refreshToken,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task DeleteByUserIdAsync(
        NpgsqlConnection connection, 
        Guid userId, 
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenAsync(
        NpgsqlConnection connection, 
        string token, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        NpgsqlConnection connection,
        RefreshToken refreshToken, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default);
}
