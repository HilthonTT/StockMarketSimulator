using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.BackgroundJobs.Users;
using Modules.Users.Domain.DomainEvents;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Quartz;
using SharedKernel;

namespace Modules.Users.Application.Users.VerifyEmail;

internal sealed class EmailVerifiedDomainEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    ISchedulerFactory schedulerFactory) : IDomainEventHandler<UserEmailVerifiedDomainEvent>
{
    public async Task Handle(UserEmailVerifiedDomainEvent notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        User? user = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user is null)
        {
            throw new DomainException(UserErrors.NotFound(notification.UserId));
        }

        IScheduler scheduler = await schedulerFactory.GetScheduler(cancellationToken);

        var jobData = new JobDataMap
        {
            { "userId", notification.UserId },
            { "email", user.Email.Value },
            { "username", user.Username.Value }
        };

        IJobDetail job = JobBuilder.Create<UserEmailVerifiedJob>()
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
