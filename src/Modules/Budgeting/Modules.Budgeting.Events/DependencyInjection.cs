using Microsoft.Extensions.DependencyInjection;

namespace Modules.Budgeting.Events;

public static class DependencyInjection
{
    public static IServiceCollection AddBudgetingEvents(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
