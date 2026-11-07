using Application.Abstractions.Events;

namespace Modules.Users.IntegrationEvents;

/// <summary>Published when a user verifies their email address.</summary>
public sealed record EmailVerifiedIntegrationEvent(Guid Id, Guid UserId) : IIntegrationEvent;
