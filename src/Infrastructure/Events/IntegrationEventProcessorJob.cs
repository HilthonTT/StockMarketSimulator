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
/// <item>Messages whose handler throws are rejected <em>with</em> requeue so they are retried on a later run;
/// the run stops at the first failure to avoid spinning on the same message.</item>
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
            logger.LogError(
                exception,
                "Failed to handle integration event {EventType} {EventId}; it will be retried",
                integrationEvent.GetType().Name,
                integrationEvent.Id);

            await channel.BasicRejectAsync(result.DeliveryTag, requeue: true, CancellationToken.None);
            return false;
        }
    }
}
