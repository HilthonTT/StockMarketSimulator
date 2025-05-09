using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Abstractions.Factories;
using Modules.Users.Application.Authentication.Factories;

namespace Modules.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(UsersApplicationAssembly.Instance));

        services.AddValidatorsFromAssembly(UsersApplicationAssembly.Instance, includeInternalTypes: true);

        services.AddScoped<IUserFactory, UserFactory>();

        return services;
    }
}
