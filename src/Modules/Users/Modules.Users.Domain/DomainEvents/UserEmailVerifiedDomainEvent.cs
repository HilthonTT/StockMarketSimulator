using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record UserEmailVerifiedDomainEvent(Guid Id, Guid UserId) : DomainEvent(Id);
