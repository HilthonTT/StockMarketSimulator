namespace Modules.Users.Contracts.Users;

public sealed record UserResponse
{
    public Guid Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public Guid? ProfileImageId { get; init; }

    public Guid? BannerImageId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}
