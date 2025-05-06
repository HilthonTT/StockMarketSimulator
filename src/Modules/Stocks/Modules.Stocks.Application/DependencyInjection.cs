using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Stocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStocksApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(StocksApplicationAssembly.Instance);
        });

        services.AddValidatorsFromAssembly(StocksApplicationAssembly.Instance, includeInternalTypes: true);

        return services;
    }
}
