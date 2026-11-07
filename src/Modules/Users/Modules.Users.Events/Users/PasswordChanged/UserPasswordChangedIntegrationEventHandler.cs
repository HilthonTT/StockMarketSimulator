using Modules.Users.IntegrationEvents;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Authentication.ChangePassword;
using Modules.Users.Application.Abstractions.BackgroundJobs;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Events.Users.PasswordChanged;

internal sealed class UserPasswordChangedIntegrationEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    IUserJobScheduler userJobScheduler) : IIntegrationEventHandler<UserPasswordChangedIntegrationEvent>
{
    public async Task Handle(UserPasswordChangedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        Option<User> optionUser = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            throw new DomainException(UserErrors.NotFound(notification.UserId));
        }
        User user = optionUser.ValueOrThrow();

        await userJobScheduler.SchedulePasswordChangedAsync(
            notification.UserId,
            user.Email.Value,
            cancellationToken);
    }
}
