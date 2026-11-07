using Modules.Users.IntegrationEvents;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.Application.Abstractions.BackgroundJobs;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Events.Users.Registered;

internal sealed class UserCreatedIntegrationEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    IUserJobScheduler userJobScheduler) : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    public async Task Handle(UserCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        Option<User> optionUser = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            throw new DomainException(UserErrors.NotFound(notification.UserId));
        }

        User user = optionUser.ValueOrThrow();

        await userJobScheduler.ScheduleEmailVerificationAsync(
            notification.UserId,
            user.Email.Value,
            notification.VerificationLink,
            cancellationToken);
    }
}
