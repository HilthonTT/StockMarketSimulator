using Application.Abstractions.Channels;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Channels;

internal sealed class ChannelNotificationsProcessor(NotificationsQueue queue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (NotificationEntry entry in queue.Reader.ReadAllAsync(stoppingToken))
        {
            await Parallel.ForEachAsync(entry.Handlers, stoppingToken, async (executor, token) =>
            {
                await executor.HandlerCallback(entry.Notification, token);
            });
        }
    }
}
