using Application.Abstractions.Events;

namespace Modules.Users.IntegrationEvents;

/// <summary>Published when a user registers. Carries the email verification link.</summary>
public sealed record UserCreatedIntegrationEvent(Guid Id, Guid UserId, string VerificationLink) : IIntegrationEvent;
