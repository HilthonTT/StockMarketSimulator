using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Users.VerifyEmail;
using Modules.Users.BackgroundJobs.Users.UserEmailVerified;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Quartz;
using SharedKernel;

namespace Modules.Users.Events.Users.EmailVerified;

internal sealed class EmailVerifiedIntegrationEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    ISchedulerFactory schedulerFactory) : IIntegrationEventHandler<EmailVerifiedIntegrationEvent>
{
    public async Task Handle(EmailVerifiedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Option<User> optionUser = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            throw new DomainException(UserErrors.NotFound(notification.UserId));
        }

        User user = optionUser.ValueOrThrow();

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
