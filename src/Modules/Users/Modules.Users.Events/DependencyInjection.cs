using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Users.Events;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersEvents(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
