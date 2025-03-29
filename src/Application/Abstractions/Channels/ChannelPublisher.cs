using MediatR;

namespace Application.Abstractions.Channels;

internal sealed class ChannelPublisher(NotificationsQueue queue) : INotificationPublisher
{
    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors, 
        INotification notification, 
        CancellationToken cancellationToken)
    {
        await queue.Writer.WriteAsync(new NotificationEntry([.. handlerExecutors], notification), cancellationToken);
    }
}
