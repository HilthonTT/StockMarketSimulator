using Application.Abstractions.Events;
using MediatR;

namespace Application.Abstractions.Messaging;

public interface IIntegrationEventHandler<in TIntegrationEvent> : INotificationHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
}