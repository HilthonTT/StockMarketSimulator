using Application.Abstractions.Events;

namespace Modules.Users.Application.Authentication.ChangePassword;

public sealed record UserPasswordChangedIntegrationEvent(Guid Id, Guid UserId) : IIntegrationEvent;
