using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Domain.DomainEvents;

namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed class TransactionSoldDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<TransactionSoldDomainEvent>
{
    public Task Handle(TransactionSoldDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new TransactionSoldIntegrationEvent(Guid.CreateVersion7(), notification.TransactionId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
