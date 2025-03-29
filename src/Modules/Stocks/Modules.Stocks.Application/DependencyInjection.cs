using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Stocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStocksApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
