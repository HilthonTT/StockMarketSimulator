﻿using Application.Abstractions.Events;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Authentication.ChangePassword;

internal sealed class UserPasswordChangedDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<UserPasswordChangedDomainEvent>
{
    public Task Handle(UserPasswordChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserPasswordChangedIntegrationEvent(Guid.CreateVersion7(), notification.UserId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
