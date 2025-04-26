using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record UserNameChangedDomainEvent(Guid UserId) : IDomainEvent;
