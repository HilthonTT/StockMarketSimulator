namespace Modules.Users.Contracts.Users;

public sealed record RegisterRequest(string Email, string Username, string Password);
