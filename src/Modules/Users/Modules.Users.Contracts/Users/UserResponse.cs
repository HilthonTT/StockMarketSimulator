namespace Modules.Users.Contracts.Users;

public sealed record UserResponse
{
    public Guid Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}
