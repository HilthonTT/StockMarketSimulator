using Application.Abstractions.Caching;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Users.Update;

/// <summary>Evicts the cached user profile so readers see the new visibility/pictures.</summary>
internal sealed class UserProfileChangedDomainEventHandler(ICacheService cacheService)
    : IDomainEventHandler<UserProfileChangedDomainEvent>
{
    public Task Handle(UserProfileChangedDomainEvent notification, CancellationToken cancellationToken) =>
        cacheService.RemoveAsync($"users:{notification.UserId}", cancellationToken);
}
