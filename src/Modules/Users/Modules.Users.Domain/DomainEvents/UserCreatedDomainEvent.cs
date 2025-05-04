using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

public sealed record UserCreatedDomainEvent(Guid Id, Guid UserId, string VerificationLink) : DomainEvent(Id);
