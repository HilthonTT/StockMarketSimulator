using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Authentication.ResendEmailVerification;

public sealed record ResendEmailVerificationCommand(string Email) : ICommand;
