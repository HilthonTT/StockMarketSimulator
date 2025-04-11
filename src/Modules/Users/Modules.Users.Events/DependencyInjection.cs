using Microsoft.Extensions.DependencyInjection;

namespace Modules.Users.Events;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersEvents(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
