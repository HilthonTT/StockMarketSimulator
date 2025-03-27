using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record UserEmailVerifiedDomainEvent(Guid UserId) : IDomainEvent;
