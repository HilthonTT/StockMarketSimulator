using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record UserCreatedDomainEvent(Guid UserId, string VerificationLink) : IDomainEvent;
