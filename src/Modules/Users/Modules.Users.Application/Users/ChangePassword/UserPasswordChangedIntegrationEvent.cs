using Application.Abstractions.Events;

namespace Modules.Users.Application.Users.ChangePassword;

public sealed record UserPasswordChangedIntegrationEvent(Guid Id, Guid UserId) : IIntegrationEvent;
