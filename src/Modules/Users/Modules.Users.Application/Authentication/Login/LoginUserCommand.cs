using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Users;

namespace Modules.Users.Application.Authentication.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<TokenResponse>;
