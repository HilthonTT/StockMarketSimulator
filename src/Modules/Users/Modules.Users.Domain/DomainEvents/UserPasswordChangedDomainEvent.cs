using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record UserPasswordChangedDomainEvent(Guid UserId) : IDomainEvent;