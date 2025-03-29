using MediatR;

namespace Application.Abstractions.Channels;

public sealed record NotificationEntry(NotificationHandlerExecutor[] Handlers, INotification Notification);
