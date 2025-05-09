using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Authentication.VerifyEmail;

public sealed record VerifyEmailCommand(Guid TokenId) : ICommand;
