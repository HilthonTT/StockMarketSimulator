namespace Modules.Users.BackgroundJobs.Users.UserEmailVerified;

public sealed class UserEmailVerifiedJobData
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;
    
    public string Username { get; init; } = string.Empty;
}
