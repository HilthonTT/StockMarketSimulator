using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Authentication.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : ICommand;
