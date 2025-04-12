using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Domain.DomainEvents;

namespace Modules.Budgeting.Application.Transactions.Buy;

internal sealed class TransactionBoughtDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<TransactionBoughtDomainEvent>
{
    public Task Handle(TransactionBoughtDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new TransactionBoughtIntegrationEvent(Guid.CreateVersion7(), notification.TransactionId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
