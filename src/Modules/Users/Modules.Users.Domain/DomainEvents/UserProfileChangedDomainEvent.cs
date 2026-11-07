using SharedKernel;

namespace Modules.Users.Domain.DomainEvents;

/// <summary>Raised when publicly visible profile data (visibility, pictures) changes.</summary>
public sealed record UserProfileChangedDomainEvent(Guid Id, Guid UserId) : DomainEvent(Id);
