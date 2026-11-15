using Modules.Users.IntegrationEvents;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Abstractions.BackgroundJobs;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Events.Users.EmailVerified;

internal sealed class EmailVerifiedIntegrationEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    IUserJobScheduler userJobScheduler) : IIntegrationEventHandler<EmailVerifiedIntegrationEvent>
{
    public async Task Handle(EmailVerifiedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var budgetProvisioner = scope.ServiceProvider.GetRequiredService<IUserBudgetProvisioner>();

        Option<User> optionUser = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            throw new DomainException(UserErrors.NotFound(notification.UserId));
        }

        User user = optionUser.ValueOrThrow();

        // The budget is created here, not in the (in-memory, never retried) Quartz job: if this throws,
        // the integration event is redelivered, and the provisioner is idempotent.
        await budgetProvisioner.EnsureBudgetAsync(notification.UserId, cancellationToken);

        await userJobScheduler.ScheduleUserEmailVerifiedAsync(
            notification.UserId,
            user.Email.Value,
            user.Username.Value,
            cancellationToken);
    }
}
