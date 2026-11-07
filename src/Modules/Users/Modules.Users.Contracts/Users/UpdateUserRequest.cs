namespace Modules.Users.Contracts.Users;

public sealed record UpdateUserRequest(string Username, bool? HasPublicProfile = null);
