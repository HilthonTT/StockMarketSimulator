using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using Application.Abstractions.Data;
using Dapper;
using Infrastructure.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly.Retry;
using Polly;
using Quartz;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Outbox;

[DisallowConcurrentExecution]
public sealed class ProcessStockOutboxMessagesJob(
    IDbConnectionFactory dbConnectionFactory,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessStockOutboxMessagesJob> logger) : IJob
{
    public const string Name = nameof(ProcessStockOutboxMessagesJob);

    private const int BatchSize = 1000;
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public async Task Execute(IJobExecutionContext context)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var stepStopwatch = new Stopwatch();

        logger.LogInformation("Beginning to process outbox messages");

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        using IDbTransaction transaction = connection.BeginTransaction();

        stepStopwatch.Restart();
        IReadOnlyList<OutboxMessageResponse> outboxMessages = await GetOutboxMessagesAsync(connection, transaction);

        if (!outboxMessages.Any())
        {
            logger.LogInformation("Completed processing outbox messages - no messages to process");
            return;
        }

        long queryTime = stepStopwatch.ElapsedMilliseconds;

        var updateQueue = new ConcurrentQueue<OutboxUpdate>();

        stepStopwatch.Restart();
        List<Task> publishTasks =
            [.. outboxMessages.Select(message => PublishMessage(message, updateQueue, context.CancellationToken))];

        await Task.WhenAll(publishTasks);
        long publishTime = stepStopwatch.ElapsedMilliseconds;

        stepStopwatch.Restart();
        if (!updateQueue.IsEmpty)
        {
            const string updateSql =
                """
                UPDATE stocks.outbox_messages
                SET processed_on_utc = v.processed_on_utc,
                    error = v.error
                FROM (VALUES
                    {0}
                ) AS v(id, processed_on_utc, error)
                WHERE outbox_messages.id = v.id::uuid
                """;

            List<OutboxUpdate> updates = [.. updateQueue];
            string valuesList = string.Join(",",
                updateQueue.Select((_, i) => $"(@Id{i}, @ProcessedOn{i}, @Error{i})"));

            var parameters = new DynamicParameters();

            for (int i = 0; i < updateQueue.Count; i++)
            {
                parameters.Add($"Id{i}", updates[i].Id.ToString());
                parameters.Add($"ProcessedOn{i}", updates[i].ProcessedOnUtc);
                parameters.Add($"Error{i}", updates[i].Error);
            }

            string formattedSql = string.Format(updateSql, valuesList);

            await connection.ExecuteAsync(formattedSql, parameters, transaction: transaction);
        }

        long updateTime = stepStopwatch.ElapsedMilliseconds;

        transaction.Commit();

        totalStopwatch.Stop();
        var totalTime = totalStopwatch.ElapsedMilliseconds;

        OutboxLoggers.LogProcessingPerformance(logger, totalTime, queryTime, publishTime, updateTime, outboxMessages.Count);
    }

    private static async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql =
            """
            SELECT id, content
            FROM stocks.outbox_messages
            WHERE processed_on_utc IS NULL
            ORDER BY created_on_utc
            LIMIT @BatchSize
            FOR UPDATE SKIP LOCKED
            """;

        IEnumerable<OutboxMessageResponse> outboxMessages = await connection.QueryAsync<OutboxMessageResponse>(
            sql,
            new { BatchSize },
            transaction: transaction);

        return [.. outboxMessages];
    }

    private async Task PublishMessage(
        OutboxMessageResponse outboxMessage,
        ConcurrentQueue<OutboxUpdate> updateQueue,
        CancellationToken cancellationToken)
    {
        const int RetryCount = 3;

        if (!TryDeserializeDomainEvent(outboxMessage.Content, out IDomainEvent? domainEvent))
        {
            return;
        }

        AsyncRetryPolicy policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(RetryCount, attempt => TimeSpan.FromMilliseconds(50 * attempt));

        PolicyResult result = await policy.ExecuteAndCaptureAsync(() =>
            publisher.Publish(domainEvent!, cancellationToken));

        var outboxUpdate = new OutboxUpdate
        {
            Id = outboxMessage.Id,
            ProcessedOnUtc = dateTimeProvider.UtcNow,
            Error = result.FinalException?.ToString(),
        };

        updateQueue.Enqueue(outboxUpdate);
    }

    private static bool TryDeserializeDomainEvent(string content, out IDomainEvent? domainEvent)
    {
        domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
            content,
            JsonSerializerSettings);

        return domainEvent is not null;
    }

    private sealed record OutboxMessageResponse(Guid Id, string Content);
}
