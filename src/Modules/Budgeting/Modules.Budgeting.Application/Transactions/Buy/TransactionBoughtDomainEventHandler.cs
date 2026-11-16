using Modules.Budgeting.IntegrationEvents;
using Application.Abstractions.Events;
using Modules.Budgeting.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.Buy;

internal sealed class TransactionBoughtDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<TransactionBoughtDomainEvent>
{
    public Task Handle(TransactionBoughtDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new TransactionBoughtIntegrationEvent(notification.Id, notification.TransactionId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
