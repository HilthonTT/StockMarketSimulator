using Dapper;
using StockMarketSimulator.Api.Infrastructure.Database;
using System.Data;
using System.Diagnostics;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure;

internal sealed class RevokeExpiredRefreshTokenBackgroundJob : BackgroundService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<RevokeExpiredRefreshTokenBackgroundJob> _logger;
    private readonly DatabaseInitializationCompletionSignal _signal;

    public RevokeExpiredRefreshTokenBackgroundJob(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<RevokeExpiredRefreshTokenBackgroundJob> logger,
        DatabaseInitializationCompletionSignal signal)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
        _signal = signal;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _signal.WaitForInitializationAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await RevokeRefreshTokensAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task RevokeRefreshTokensAsync(CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Revoke expired tokens
            int expiredCount = await RevokeExpiredTokensAsync(connection, transaction);

            // Revoke duplicate tokens (keep latest per user)
            int duplicatesCount = await RevokeDuplicateTokensAsync(connection, transaction);

            await transaction.CommitAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Revoked {ExpiredCount} expired tokens and {DuplicatesCount} duplicate tokens in {ElapsedMs}ms.",
                expiredCount,
                duplicatesCount,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while revoking refresh tokens.");
            throw;
        }
    }

    private static async Task<int> RevokeExpiredTokensAsync(IDbConnection connection, IDbTransaction transaction)
    {
        const string sql =
            """
            DELETE FROM refresh_tokens
            WHERE expires_on_utc < @Now
            """;

        int affectedRows = await connection.ExecuteAsync(
            sql,
            new { Now = DateTime.UtcNow },
            transaction: transaction);

        return affectedRows;
    }

    private static async Task<int> RevokeDuplicateTokensAsync(IDbConnection connection, IDbTransaction transaction)
    {
        const string fetchSql = 
            """
            WITH ranked_tokens AS (
                SELECT id, user_id, expires_on_utc,
                       ROW_NUMBER() OVER (PARTITION BY user_id ORDER BY expires_on_utc DESC) AS rank
                FROM refresh_tokens
            )
            SELECT id FROM ranked_tokens
            WHERE rank > 1
            """;

        IEnumerable<Guid> duplicateIds = await connection.QueryAsync<Guid>(fetchSql, transaction: transaction);

        if (!duplicateIds.Any())
        {
            return 0;
        }

        const string deleteSql = 
            """
            DELETE FROM refresh_tokens
            WHERE id = ANY(@Ids)
            """;

        int affectedRows = await connection.ExecuteAsync(
            deleteSql,
            new { Ids = duplicateIds.ToArray() },
            transaction: transaction);

        return affectedRows;
    }
}
