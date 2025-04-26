using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Modules.Users.Domain.DomainEvents;

namespace Modules.Users.Application.Users.Update;

internal sealed class UserNameChangedDomainEventHandler(ICacheService cacheService) 
    : IDomainEventHandler<UserNameChangedDomainEvent>
{
    public Task Handle(UserNameChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        string cacheKey = $"users:{notification.UserId}";

        return cacheService.RemoveAsync(cacheKey, cancellationToken);
    }
}
