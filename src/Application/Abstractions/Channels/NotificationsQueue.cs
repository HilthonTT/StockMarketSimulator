using System.Threading.Channels;

namespace Application.Abstractions.Channels;

public sealed class NotificationsQueue(int capacity = 100)
{
    private readonly Channel<NotificationEntry> _queue = 
        Channel.CreateBounded<NotificationEntry>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });

    public ChannelReader<NotificationEntry> Reader => _queue.Reader;

    public ChannelWriter<NotificationEntry> Writer => _queue.Writer;
}
