using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Authentication.Register;

public sealed record RegisterUserCommand(string Email, string Username, string Password) : ICommand<Guid>;
