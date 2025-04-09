using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Users.ResendEmailVerification;

public sealed record ResendEmailVerificationCommand(string Email) : ICommand;
