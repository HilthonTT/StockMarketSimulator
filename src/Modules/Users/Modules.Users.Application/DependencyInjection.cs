using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(UsersApplicationAssembly.Instance));

        services.AddValidatorsFromAssembly(UsersApplicationAssembly.Instance, includeInternalTypes: true);

        return services;
    }
}
