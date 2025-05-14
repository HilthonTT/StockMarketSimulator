using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Budgeting.Events;

public static class DependencyInjection
{
    public static IServiceCollection AddBudgetingEvents(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
