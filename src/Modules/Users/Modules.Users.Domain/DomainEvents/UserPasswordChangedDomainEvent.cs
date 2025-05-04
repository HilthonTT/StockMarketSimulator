using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record UserPasswordChangedDomainEvent(Guid Id, Guid UserId) : DomainEvent(Id);