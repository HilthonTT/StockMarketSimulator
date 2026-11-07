using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Outbox;

/// <summary>A module schema that owns an <c>outbox_messages</c> table (registered by each module).</summary>
public sealed record OutboxSchema(string Name);

/// <summary>Failed outbox messages in one schema.</summary>
public sealed record OutboxFailureSummary(string Schema, int FailedCount, DateTime? OldestFailedOnUtc);

/// <summary>
/// Inspects and re-drives outbox messages whose handlers kept failing.
/// </summary>
/// <remarks>
/// <see cref="OutboxProcessorJob"/> stamps a message that still fails after its retries with
/// <c>processed_on_utc</c> and <c>error</c>, so it can't block the outbox. Re-driving clears both columns,
/// and the next outbox run dispatches the message again. Handlers that already succeeded for it are
/// skipped by <c>IdempotentDomainEventHandler</c>, so only the failed ones run again.
/// </remarks>
public sealed class OutboxAdministration(
    IDbConnectionFactory dbConnectionFactory,
    IEnumerable<OutboxSchema> schemas,
    ILogger<OutboxAdministration> logger)
{
    private readonly HashSet<string> _schemas = [.. schemas.Select(schema => schema.Name)];

    public IReadOnlyCollection<string> Schemas => _schemas;

    public bool IsKnownSchema(string schema) => _schemas.Contains(schema);

    public async Task<IReadOnlyList<OutboxFailureSummary>> GetFailuresAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        var summaries = new List<OutboxFailureSummary>(_schemas.Count);

        foreach (string schema in _schemas.Order(StringComparer.Ordinal))
        {
            // The schema name is never user input here: it comes from the registered allow-list.
            string sql =
                $"""
                SELECT COUNT(*)::int AS FailedCount, MIN(processed_on_utc) AS OldestFailedOnUtc
                FROM {schema}.outbox_messages
                WHERE error IS NOT NULL
                """;

            FailureRow row = await connection.QuerySingleAsync<FailureRow>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            summaries.Add(new OutboxFailureSummary(schema, row.FailedCount, row.OldestFailedOnUtc));
        }

        return summaries;
    }

    /// <summary>
    /// Resets every failed message in <paramref name="schema"/> so the outbox job processes it again.
    /// </summary>
    /// <returns>The number of messages re-driven.</returns>
    /// <exception cref="ArgumentException">The schema isn't a registered outbox schema.</exception>
    public async Task<int> RedriveFailedAsync(string schema, CancellationToken cancellationToken = default)
    {
        if (!IsKnownSchema(schema))
        {
            throw new ArgumentException($"'{schema}' is not a registered outbox schema.", nameof(schema));
        }

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        string sql =
            $"""
            UPDATE {schema}.outbox_messages
            SET processed_on_utc = NULL,
                error = NULL
            WHERE error IS NOT NULL
            """;

        int count = await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));

        logger.LogInformation("Re-drove {Count} failed outbox messages in schema {Schema}", count, schema);

        return count;
    }

    private sealed class FailureRow
    {
        public int FailedCount { get; init; }

        public DateTime? OldestFailedOnUtc { get; init; }
    }
}

public static class OutboxServiceCollectionExtensions
{
    /// <summary>Registers a module's outbox schema for monitoring and re-drive.</summary>
    public static IServiceCollection AddOutboxSchema(this IServiceCollection services, string schema)
    {
        services.AddSingleton(new OutboxSchema(schema));

        return services;
    }
}
