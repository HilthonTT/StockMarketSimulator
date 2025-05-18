using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record FollowerCreatedDomainEvent(Guid Id, Guid UserId, Guid FollowedId) : DomainEvent(Id);
