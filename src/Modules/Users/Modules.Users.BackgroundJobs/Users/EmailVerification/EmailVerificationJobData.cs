namespace Modules.Users.BackgroundJobs.Users.EmailVerification;

public sealed class EmailVerificationJobData
{
    public Guid UserId { get; init; }

    public string? Email { get; init; } = string.Empty;

    public string? VerificationLink { get; init; } = string.Empty;
}
