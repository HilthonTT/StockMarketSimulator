using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel;

namespace Modules.Users.BackgroundJobs.Users.RevokeExpiredRefreshTokens;

/// <summary>
/// Deletes expired refresh tokens, and caps how many live sessions (devices) one user can hold.
/// </summary>
/// <remarks>
/// Unexpired tokens are never removed just because the user has a newer one, so signing in on a second
/// device doesn't log out the first. Only when a user exceeds <see cref="MaxTokensPerUser"/> live tokens
/// are the ones closest to expiry removed.
/// </remarks>
[DisallowConcurrentExecution]
public sealed class RevokeExpiredRefreshTokenJob(
    IDateTimeProvider dateTimeProvider,
    IDbConnectionFactory dbConnectionFactory,
    ILogger<RevokeExpiredRefreshTokenJob> logger) : IJob
{
    public const string Name = nameof(RevokeExpiredRefreshTokenJob);

    /// <summary>The maximum number of live refresh tokens (sessions) kept per user.</summary>
    public const int MaxTokensPerUser = 10;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        (int expiredCount, int overCapCount) = await CleanUpAsync(connection, dateTimeProvider.UtcNow, cancellationToken);

        if (expiredCount > 0 || overCapCount > 0)
        {
            logger.LogInformation(
                "Deleted {ExpiredCount} expired refresh tokens and {OverCapCount} tokens over the per-user cap of {MaxTokensPerUser}.",
                expiredCount,
                overCapCount,
                MaxTokensPerUser);
        }
    }

    /// <summary>
    /// Runs the cleanup in one transaction. Exposed for tests.
    /// </summary>
    public static async Task<(int ExpiredCount, int OverCapCount)> CleanUpAsync(
        IDbConnection connection,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        using IDbTransaction transaction = connection.BeginTransaction();

        const string deleteExpiredSql =
            """
            DELETE FROM users.refresh_tokens
            WHERE expires_on_utc <= @Now;
            """;

        int expiredCount = await connection.ExecuteAsync(new CommandDefinition(
            deleteExpiredSql,
            new { Now = utcNow },
            transaction,
            cancellationToken: cancellationToken));

        const string deleteOverCapSql =
            """
            DELETE FROM users.refresh_tokens AS t
            USING (
                SELECT id,
                       ROW_NUMBER() OVER (PARTITION BY user_id ORDER BY expires_on_utc DESC, id DESC) AS rank
                FROM users.refresh_tokens
            ) AS ranked
            WHERE t.id = ranked.id AND ranked.rank > @MaxTokensPerUser;
            """;

        int overCapCount = await connection.ExecuteAsync(new CommandDefinition(
            deleteOverCapSql,
            new { MaxTokensPerUser },
            transaction,
            cancellationToken: cancellationToken));

        transaction.Commit();

        return (expiredCount, overCapCount);
    }
}
