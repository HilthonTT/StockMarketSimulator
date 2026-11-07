using Application.Abstractions.Events;

namespace Modules.Users.IntegrationEvents;

/// <summary>Published when a user changes their password.</summary>
public sealed record UserPasswordChangedIntegrationEvent(Guid Id, Guid UserId) : IIntegrationEvent;
