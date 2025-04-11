using Application.Abstractions.Events;

namespace Modules.Users.Application.Users.Register;

public sealed record UserCreatedIntegrationEvent(Guid Id, Guid UserId, string VerificationLink) : IIntegrationEvent;
