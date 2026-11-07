using Application.Abstractions.Events;
using Infrastructure.Events.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Events;

/// <summary>
/// Publishes integration events to RabbitMQ.
/// </summary>
/// <remarks>
/// Registered as a singleton. The connection and channel are created lazily once and reused;
/// a RabbitMQ <see cref="IChannel"/> is not safe for concurrent publishing, so publishes are serialized
/// through a semaphore (this also guards the lazy initialization against races).
/// </remarks>
internal sealed class EventBus(
    IConfiguration configuration,
    IOptions<MessageBrokerOptions> options,
    ILogger<EventBus> logger) : IEventBus, IAsyncDisposable
{
    private readonly MessageBrokerOptions _options = options.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly ConnectionFactory _connectionFactory = new()
    {
        Uri = new Uri(configuration.GetConnectionString(ConfigurationNames.RabbitMq) ??
            throw new InvalidOperationException("RabbitMQ connection string not found."))
    };

    private IConnection? _connection;
    private IChannel? _channel;

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent
    {
        byte[] body = IntegrationEventSerializer.Serialize(integrationEvent);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            IChannel channel = await GetChannelAsync(cancellationToken);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                mandatory: false,
                basicProperties: new BasicProperties { Persistent = true, ContentType = "application/json" },
                body: body,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish event of type {EventType}", typeof(T).Name);

            // Drop a broken channel/connection so the next publish reconnects.
            await ResetAsync();
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ResetAsync();
        _lock.Dispose();
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await ResetAsync();

        _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        return _channel;
    }

    private async Task ResetAsync()
    {
        try
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "An error occurred while disposing the RabbitMQ connection.");
        }
        finally
        {
            _channel = null;
            _connection = null;
        }
    }
}
