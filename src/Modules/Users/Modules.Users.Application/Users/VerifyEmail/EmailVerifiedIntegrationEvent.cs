using Application.Abstractions.Events;

namespace Modules.Users.Application.Users.VerifyEmail;

public sealed record EmailVerifiedIntegrationEvent(Guid Id, Guid UserId) : IIntegrationEvent;
