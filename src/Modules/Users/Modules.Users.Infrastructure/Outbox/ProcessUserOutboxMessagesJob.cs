using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using Application.Abstractions.Data;
using Dapper;
using Infrastructure.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using SharedKernel;

namespace Modules.Users.Infrastructure.Outbox;

[DisallowConcurrentExecution]
public sealed partial class ProcessUserOutboxMessagesJob(
    IDbConnectionFactory dbConnectionFactory,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessUserOutboxMessagesJob> logger) : IJob
{
    public const string Name = nameof(ProcessUserOutboxMessagesJob);

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
                UPDATE users.outbox_messages
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
            FROM users.outbox_messages
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
        try
        { 
            IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                outboxMessage.Content,
                JsonSerializerSettings)!;

            await publisher.Publish(domainEvent, cancellationToken);

            updateQueue.Enqueue(new OutboxUpdate { Id = outboxMessage.Id, ProcessedOnUtc = dateTimeProvider.UtcNow });
        }
        catch (Exception ex)
        {
            var update = new OutboxUpdate
            {
                Id = outboxMessage.Id,
                ProcessedOnUtc = dateTimeProvider.UtcNow,
                Error = ex.ToString()
            };
            updateQueue.Enqueue(update);
        }
    }

    private sealed record OutboxMessageResponse(Guid Id, string Content);
}
