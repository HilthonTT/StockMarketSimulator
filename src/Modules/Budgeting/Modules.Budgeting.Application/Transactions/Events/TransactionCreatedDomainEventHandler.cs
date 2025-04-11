using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Domain.DomainEvents;

namespace Modules.Budgeting.Application.Transactions.Events;

internal sealed class TransactionCreatedDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<TransactionCreatedDomainEvent>
{
    public Task Handle(TransactionCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
