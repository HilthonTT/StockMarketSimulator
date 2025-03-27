namespace Modules.Users.Contracts.Users;

public sealed record UserResponse(Guid Id, string Username, string Email, DateTime CreatedOnUtc, DateTime? ModifiedOnutc);
