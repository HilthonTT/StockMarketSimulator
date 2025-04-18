using System.Text;
using Application.Abstractions.Events;
using Infrastructure.Events.Options;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using RabbitMQ.Client;

namespace Infrastructure.Events;

[DisallowConcurrentExecution]
public sealed class IntegrationEventProcessorJob(
    IConfiguration configuration, 
    IServiceProvider serviceProvider,
    ILogger<IntegrationEventProcessorJob> logger,
    IOptions<MessageBrokerOptions> options) : IJob, IAsyncDisposable
{
    public const string Name = nameof(IntegrationEventProcessorJob);

    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly MessageBrokerOptions _options = options.Value;
    private readonly ConnectionFactory _connectionFactory = new()
    {
        Uri = new Uri(configuration.GetConnectionString(ConfigurationNames.RabbitMq) ??
            throw new InvalidOperationException("RabbitMQ connection string not found."))
    };

    private IConnection? _connection;
    private IChannel? _channel;

    public async Task Execute(IJobExecutionContext context)
    {
        _connection ??= await _connectionFactory.CreateConnectionAsync(context.CancellationToken);
        _channel ??= await _connection.CreateChannelAsync(cancellationToken: context.CancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: context.CancellationToken);

        BasicGetResult? result =
            await _channel.BasicGetAsync(_options.QueueName, autoAck: false, cancellationToken: context.CancellationToken);

        if (result is null)
        {
            logger.LogDebug("No messages found in queue.");
            return;
        }

        string body = Encoding.UTF8.GetString(result.Body.Span);

        IIntegrationEvent? integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
            body, _jsonSerializerSettings);

        if (integrationEvent is null)
        {
            logger.LogWarning("Failed to deserialize integration event.");
            return;
        }

        using IServiceScope scope = serviceProvider.CreateScope();
        IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        await publisher.Publish(integrationEvent, context.CancellationToken);

        await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: context.CancellationToken);
    }

    public async ValueTask DisposeAsync()
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
    }
}
