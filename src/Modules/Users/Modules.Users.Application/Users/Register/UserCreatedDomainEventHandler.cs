using Application.Abstractions.Messaging;
using Modules.Users.BackgroundJobs.Users;
using Modules.Users.Domain.DomainEvents;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Quartz;
using SharedKernel;

namespace Modules.Users.Application.Users.Register;

internal sealed class UserCreatedDomainEventHandler(
    IUserRepository userRepository, 
    ISchedulerFactory schedulerFactory) : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user is null)
        {
            throw new DomainException(UserErrors.NotFound(notification.UserId));
        }

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

        var jobData = new JobDataMap
        {
            { "userId", notification.UserId },
            { "email", user.Email.Value },
            { "verification-link", notification.VerificationLink }
        };

        IJobDetail job = JobBuilder.Create<EmailVerificationJob>()
            .WithIdentity($"reminder-{Guid.CreateVersion7()}", "email-reminders")
            .SetJobData(jobData)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger-{Guid.CreateVersion7()}", "email-reminders")
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }
}
