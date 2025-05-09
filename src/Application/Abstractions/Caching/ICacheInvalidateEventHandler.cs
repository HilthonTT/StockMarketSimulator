using MediatR;

namespace Application.Abstractions.Caching;

public interface ICacheInvalidateEventHandler<in TCacheInvalidateEvent> : INotificationHandler<TCacheInvalidateEvent>
    where TCacheInvalidateEvent : ICacheInvalidateEvent
{
}
