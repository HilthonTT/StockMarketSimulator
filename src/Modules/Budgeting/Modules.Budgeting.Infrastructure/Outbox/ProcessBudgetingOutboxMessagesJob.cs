using System.Data;
using Application.Abstractions.Data;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Outbox;

[DisallowConcurrentExecution]
public sealed class ProcessBudgetingOutboxMessagesJob(
    IDbConnectionFactory dbConnectionFactory,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessBudgetingOutboxMessagesJob> logger) : IJob
{
    public const string Name = nameof(ProcessBudgetingOutboxMessagesJob);

    private const int BatchSize = 15;
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Beginning to process outbox messages");

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        using IDbTransaction transaction = connection.BeginTransaction();

        IReadOnlyList<OutboxMessageResponse> outboxMessages = await GetOutboxMessagesAsync(connection, transaction);

        if (!outboxMessages.Any())
        {
            logger.LogInformation("Completed processing outbox messages - no messages to process");
            return;
        }

        foreach (OutboxMessageResponse outboxMessage in outboxMessages)
        {
            Exception? exception = null;

            try
            {
                IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                outboxMessage.Content,
                    JsonSerializerSettings)!;

                await publisher.Publish(domainEvent);
            }
            catch (Exception caughtException)
            {
                logger.LogError(
                    caughtException,
                    "Exception while processing outbox message {MessageId}",
                    outboxMessage.Id);

                exception = caughtException;
            }

            await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception);
        }

        transaction.Commit();

        logger.LogInformation("Completed processing outbox messages");
    }

    private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql =
            """
            SELECT id, content
            FROM budgeting.outbox_messages
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

    private Task UpdateOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        OutboxMessageResponse outboxMessage,
        Exception? exception)
    {
        const string sql =
            """
            UPDATE budgeting.outbox_messages
            SET processed_on_utc = @ProcessedOnUtc,
                error = @Error
            WHERE id = @Id
            """;

        return connection.ExecuteAsync(
            sql,
            new
            {
                outboxMessage.Id,
                ProcessedOnUtc = dateTimeProvider.UtcNow,
                Error = exception?.ToString()
            },
            transaction: transaction);
    }

    private sealed record OutboxMessageResponse(Guid Id, string Content);
}
