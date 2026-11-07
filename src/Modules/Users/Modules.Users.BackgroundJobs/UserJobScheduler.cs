using Modules.Users.Application.Abstractions.BackgroundJobs;
using Modules.Users.BackgroundJobs.Users.EmailVerification;
using Modules.Users.BackgroundJobs.Users.PasswordChanged;
using Modules.Users.BackgroundJobs.Users.UserEmailVerified;
using Quartz;

namespace Modules.Users.BackgroundJobs;

/// <summary>
/// Quartz implementation of <see cref="IUserJobScheduler"/>. The job data keys here must match what
/// each job reads from its <see cref="JobDataMap"/>.
/// </summary>
internal sealed class UserJobScheduler(ISchedulerFactory schedulerFactory) : IUserJobScheduler
{
    private const string Group = "email-reminders";

    public Task ScheduleEmailVerificationAsync(
        Guid userId,
        string email,
        string verificationLink,
        CancellationToken cancellationToken = default) =>
        ScheduleNowAsync<EmailVerificationJob>(
            new JobDataMap
            {
                { "userId", userId },
                { "email", email },
                { "verification-link", verificationLink }
            },
            cancellationToken);

    public Task ScheduleUserEmailVerifiedAsync(
        Guid userId,
        string email,
        string username,
        CancellationToken cancellationToken = default) =>
        ScheduleNowAsync<UserEmailVerifiedJob>(
            new JobDataMap
            {
                { "userId", userId },
                { "email", email },
                { "username", username }
            },
            cancellationToken);

    public Task SchedulePasswordChangedAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default) =>
        ScheduleNowAsync<PasswordChangedJob>(
            new JobDataMap
            {
                { "userId", userId },
                { "email", email }
            },
            cancellationToken);

    private async Task ScheduleNowAsync<TJob>(JobDataMap jobData, CancellationToken cancellationToken)
        where TJob : IJob
    {
        IScheduler scheduler = await schedulerFactory.GetScheduler(cancellationToken);

        IJobDetail job = JobBuilder.Create<TJob>()
            .WithIdentity($"{typeof(TJob).Name}-{Guid.CreateVersion7()}", Group)
            .UsingJobData(jobData)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger-{Guid.CreateVersion7()}", Group)
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken: cancellationToken);
    }
}
