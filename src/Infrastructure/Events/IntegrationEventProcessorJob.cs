using Application.Abstractions.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Events;

[DisallowConcurrentExecution]
public sealed class IntegrationEventProcessorJob(
    InMemoryMessageQueue queue,
    IPublisher publisher,
    ILogger<IntegrationEventProcessorJob> logger) : IJob
{
    public const string Name = nameof(IntegrationEventProcessorJob);

    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken stoppingToken = context.CancellationToken;

        await foreach (IIntegrationEvent integrationEvent in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await publisher.Publish(integrationEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something went wrong! {IntegrationEventId}", integrationEvent.Id);
            }
        }
    }
}
