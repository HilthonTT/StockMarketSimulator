namespace Modules.Users.BackgroundJobs.Users.PasswordChanged;

public sealed class PasswordChangedJobData
{
    public Guid UserId { get; init; }

    public string? Email { get; init; } = string.Empty;
}
