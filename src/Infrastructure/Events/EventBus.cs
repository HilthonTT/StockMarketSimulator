using System.Text;
using Application.Abstractions.Events;
using Infrastructure.Events.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Infrastructure.Events;

internal sealed class EventBus(
    IConfiguration configuration, 
    IOptions<MessageBrokerOptions> options,
    ILogger<EventBus> logger) : IEventBus, IAsyncDisposable
{
    private readonly MessageBrokerOptions _options = options.Value;
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

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
        try
        {
            _connection ??= await _connectionFactory.CreateConnectionAsync(cancellationToken);
            _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            string payload = JsonConvert.SerializeObject(integrationEvent, typeof(IIntegrationEvent), _jsonSerializerSettings);
            byte[] body = Encoding.UTF8.GetBytes(payload);

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: _options.QueueName,
                body: body,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish event of type {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            logger.LogInformation("RabbitMQ connection and channel disposed.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "An error occurred while disposing the EventBus.");
        }
    }
}
