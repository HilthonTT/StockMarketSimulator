using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Application.Abstractions.Data;
using Dapper;
using Infrastructure.DomainEvents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Quartz;
using SharedKernel;

namespace Infrastructure.Outbox;

/// <summary>
/// Shared implementation of the transactional-outbox processor.
/// </summary>
/// <remarks>
/// Each module owns an <c>outbox_messages</c> table in its own schema. A module only has to derive
/// from this class and say which schema it reads and which assembly its domain events live in.
/// <para>
/// Per run the job locks a batch of unprocessed rows (<c>FOR UPDATE SKIP LOCKED</c>, so several
/// instances can run side by side), dispatches every event, then stamps all rows in a single
/// <c>UPDATE</c>. A message that cannot be deserialized or whose handlers keep failing is marked
/// processed with its error, so one poison message can never block the rest of the outbox.
/// </para>
/// </remarks>
[DisallowConcurrentExecution]
public abstract class OutboxProcessorJob(
    IDbConnectionFactory dbConnectionFactory,
    IDomainEventsDispatcher domainEventsDispatcher,
    IDateTimeProvider dateTimeProvider,
    ILogger logger) : IJob
{
    private const int BatchSize = 1000;
    private const int RetryCount = 3;

    private static readonly ConcurrentDictionary<Assembly, JsonSerializerSettings> SerializerSettings = new();

    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(RetryCount, attempt => TimeSpan.FromMilliseconds(50 * attempt));

    /// <summary>The database schema that owns the <c>outbox_messages</c> table, e.g. <c>users</c>.</summary>
    protected abstract string Schema { get; }

    /// <summary>The assembly that contains the module's domain event types.</summary>
    protected abstract Assembly DomainAssembly { get; }

    /// <summary>How many events are dispatched concurrently.</summary>
    protected virtual int MaxDegreeOfParallelism => Environment.ProcessorCount;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var stepStopwatch = Stopwatch.StartNew();

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);
        using IDbTransaction transaction = connection.BeginTransaction();

        IReadOnlyList<OutboxMessageResponse> outboxMessages =
            await GetOutboxMessagesAsync(connection, transaction, cancellationToken);

        if (outboxMessages.Count == 0)
        {
            return;
        }

        long queryTime = stepStopwatch.ElapsedMilliseconds;
        stepStopwatch.Restart();

        var updateQueue = new ConcurrentQueue<OutboxUpdate>();

        await Parallel.ForEachAsync(
            outboxMessages,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            },
            async (message, token) => updateQueue.Enqueue(await PublishMessageAsync(message, token)));

        long publishTime = stepStopwatch.ElapsedMilliseconds;
        stepStopwatch.Restart();

        await UpdateOutboxMessagesAsync(connection, transaction, [.. updateQueue], cancellationToken);

        long updateTime = stepStopwatch.ElapsedMilliseconds;

        transaction.Commit();

        OutboxLoggers.LogProcessingPerformance(
            logger,
            totalStopwatch.ElapsedMilliseconds,
            queryTime,
            publishTime,
            updateTime,
            outboxMessages.Count);
    }

    private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        string sql =
            $"""
            SELECT id AS {nameof(OutboxMessageResponse.Id)}, content AS {nameof(OutboxMessageResponse.Content)}
            FROM {Schema}.outbox_messages
            WHERE processed_on_utc IS NULL
            ORDER BY created_on_utc
            LIMIT @BatchSize
            FOR UPDATE SKIP LOCKED
            """;

        IEnumerable<OutboxMessageResponse> messages = await connection.QueryAsync<OutboxMessageResponse>(
            new CommandDefinition(sql, new { BatchSize }, transaction, cancellationToken: cancellationToken));

        return [.. messages];
    }

    private async Task<OutboxUpdate> PublishMessageAsync(
        OutboxMessageResponse outboxMessage,
        CancellationToken cancellationToken)
    {
        string? error;

        try
        {
            IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    outboxMessage.Content,
                    GetSerializerSettings(DomainAssembly))
                ?? throw new Newtonsoft.Json.JsonSerializationException("Outbox message content deserialized to null.");

            PolicyResult result = await RetryPolicy.ExecuteAndCaptureAsync(
                token => domainEventsDispatcher.DispatchAsync(domainEvent, token),
                cancellationToken);

            error = result.FinalException?.ToString();
        }
        catch (Newtonsoft.Json.JsonException exception)
        {
            // A message that can never be read must not block the rest of the outbox.
            error = exception.ToString();
        }

        if (error is not null)
        {
            logger.LogError(
                "Outbox message {MessageId} in schema {Schema} failed: {Error}",
                outboxMessage.Id,
                Schema,
                error);
        }

        return new OutboxUpdate
        {
            Id = outboxMessage.Id,
            ProcessedOnUtc = dateTimeProvider.UtcNow,
            Error = error
        };
    }

    private async Task UpdateOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<OutboxUpdate> updates,
        CancellationToken cancellationToken)
    {
        if (updates.Count == 0)
        {
            return;
        }

        var parameters = new DynamicParameters();
        var values = new List<string>(updates.Count);

        for (int i = 0; i < updates.Count; i++)
        {
            parameters.Add($"Id{i}", updates[i].Id);
            parameters.Add($"ProcessedOn{i}", updates[i].ProcessedOnUtc);
            parameters.Add($"Error{i}", updates[i].Error, DbType.String);

            values.Add($"(@Id{i}, @ProcessedOn{i}, @Error{i})");
        }

        string sql =
            $"""
            UPDATE {Schema}.outbox_messages AS o
            SET processed_on_utc = v.processed_on_utc,
                error = v.error
            FROM (VALUES
                {string.Join(",\n    ", values)}
            ) AS v(id, processed_on_utc, error)
            WHERE o.id = v.id
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
    }

    private static JsonSerializerSettings GetSerializerSettings(Assembly assembly) =>
        SerializerSettings.GetOrAdd(assembly, a => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = { new DomainEventConverter(a) }
        });

    private sealed record OutboxMessageResponse(Guid Id, string Content);
}
