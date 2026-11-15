using System.Text;
using Application.Abstractions.Events;
using Infrastructure.Events.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using RabbitMQ.Client;

namespace Infrastructure.Events;

/// <summary>
/// Drains the integration-event queue and dispatches each event to its in-process handlers.
/// </summary>
/// <remarks>
/// Quartz creates a new job instance per run, so each run opens one connection, processes up to
/// <see cref="MaxMessagesPerRun"/> messages and disposes it.
/// <list type="bullet">
/// <item>Messages that cannot be deserialized are rejected without requeue (they would fail forever).</item>
/// <item>Messages whose handler throws are re-published to the <em>tail</em> of the queue with an incremented
/// <see cref="AttemptHeader"/> (a plain requeue would put them back at the head and block everything behind
/// them); the run stops at the first failure so the retry waits for the next run. After
/// <see cref="MaxAttempts"/> attempts the message is discarded and logged as an error.</item>
/// </list>
/// </remarks>
[DisallowConcurrentExecution]
public sealed class IntegrationEventProcessorJob(
    IConfiguration configuration,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<IntegrationEventProcessorJob> logger,
    IOptions<MessageBrokerOptions> options) : IJob
{
    public const string Name = nameof(IntegrationEventProcessorJob);

    private const int MaxMessagesPerRun = 100;

    private readonly MessageBrokerOptions _options = options.Value;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString(ConfigurationNames.RabbitMq) ??
                throw new InvalidOperationException("RabbitMQ connection string not found."))
        };

        await using IConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        for (int processed = 0; processed < MaxMessagesPerRun; processed++)
        {
            BasicGetResult? result =
                await channel.BasicGetAsync(_options.QueueName, autoAck: false, cancellationToken: cancellationToken);

            if (result is null)
            {
                return;
            }

            if (!await TryProcessAsync(channel, result, cancellationToken))
            {
                return;
            }
        }
    }

    private async Task<bool> TryProcessAsync(IChannel channel, BasicGetResult result, CancellationToken cancellationToken)
    {
        IIntegrationEvent? integrationEvent;

        try
        {
            integrationEvent = IntegrationEventSerializer.Deserialize(result.Body.Span);
        }
        catch (JsonException exception)
        {
            integrationEvent = null;
            logger.LogError(exception, "Failed to deserialize integration event {DeliveryTag}", result.DeliveryTag);
        }

        if (integrationEvent is null)
        {
            // Poison message: requeueing would make it fail forever.
            await channel.BasicRejectAsync(result.DeliveryTag, requeue: false, cancellationToken);
            return true;
        }

        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            IIntegrationEventsDispatcher dispatcher =
                scope.ServiceProvider.GetRequiredService<IIntegrationEventsDispatcher>();

            await dispatcher.DispatchAsync(integrationEvent, cancellationToken);

            await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            int attempt = GetAttempt(result.BasicProperties.Headers);

            if (ShouldDiscard(attempt))
            {
                logger.LogError(
                    exception,
                    "Discarding integration event {EventType} {EventId} after {Attempts} failed attempts. Body: {Body}",
                    integrationEvent.GetType().Name,
                    integrationEvent.Id,
                    attempt,
                    Encoding.UTF8.GetString(result.Body.Span));

                await channel.BasicRejectAsync(result.DeliveryTag, requeue: false, CancellationToken.None);
                return false;
            }

            logger.LogWarning(
                exception,
                "Failed to handle integration event {EventType} {EventId} (attempt {Attempt} of {MaxAttempts}); it will be retried",
                integrationEvent.GetType().Name,
                integrationEvent.Id,
                attempt,
                MaxAttempts);

            // Re-publish to the tail with the attempt count, then ack the original. A plain requeue would put
            // the message back at the head, so one failing event would block every event behind it.
            var properties = new BasicProperties(result.BasicProperties)
            {
                Headers = new Dictionary<string, object?>(result.BasicProperties.Headers ?? new Dictionary<string, object?>())
                {
                    [AttemptHeader] = attempt + 1
                }
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                mandatory: false,
                basicProperties: properties,
                body: result.Body,
                cancellationToken: CancellationToken.None);

            await channel.BasicAckAsync(result.DeliveryTag, multiple: false, CancellationToken.None);

            // Stop this run so the retry waits for the next one instead of spinning.
            return false;
        }
    }

    /// <summary>Header carrying how many times delivery has been attempted (1 for the first delivery).</summary>
    public const string AttemptHeader = "x-attempt";

    /// <summary>How many times an event is handled before it is discarded with an error log.</summary>
    public const int MaxAttempts = 5;

    public static bool ShouldDiscard(int attempt) => attempt >= MaxAttempts;

    /// <summary>Reads <see cref="AttemptHeader"/>; a message without it is on its first attempt.</summary>
    public static int GetAttempt(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue(AttemptHeader, out object? value) || value is null)
        {
            return 1;
        }

        int attempt = value switch
        {
            int i => i,
            long l => (int)Math.Min(l, int.MaxValue),
            short s => s,
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out int parsed) => parsed,
            string text when int.TryParse(text, out int parsed) => parsed,
            _ => 1
        };

        return Math.Max(attempt, 1);
    }
}
