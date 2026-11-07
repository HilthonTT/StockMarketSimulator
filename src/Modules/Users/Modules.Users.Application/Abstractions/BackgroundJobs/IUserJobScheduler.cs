namespace Modules.Users.Application.Abstractions.BackgroundJobs;

/// <summary>
/// Schedules the Users module's one-off background jobs (emails, welcome flow).
/// </summary>
/// <remarks>
/// Keeps the Application layer free of Quartz: the implementation lives in
/// <c>Modules.Users.BackgroundJobs</c>, next to the jobs themselves.
/// </remarks>
public interface IUserJobScheduler
{
    Task ScheduleEmailVerificationAsync(
        Guid userId,
        string email,
        string verificationLink,
        CancellationToken cancellationToken = default);

    Task ScheduleUserEmailVerifiedAsync(
        Guid userId,
        string email,
        string username,
        CancellationToken cancellationToken = default);

    Task SchedulePasswordChangedAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default);
}
