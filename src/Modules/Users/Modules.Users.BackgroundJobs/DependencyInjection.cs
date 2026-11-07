using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Abstractions.BackgroundJobs;

namespace Modules.Users.BackgroundJobs;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersBackgroundJobs(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(UsersBackgroundJobsAssembly.Instance, includeInternalTypes: true);

        services.AddSingleton<IUserJobScheduler, UserJobScheduler>();

        return services;
    }
}
