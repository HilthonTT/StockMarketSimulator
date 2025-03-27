using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Users.VerifyEmail;

public sealed record VerifyEmailCommand(Guid TokenId) : ICommand;
