using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Users.BackgroundJobs;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersBackgroundJobs(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(UsersBackgroundJobsAssembly.Instance, includeInternalTypes: true);

        return services;
    }
}
