using StockMarketSimulator.Api.Modules.Roles.Api;
using StockMarketSimulator.Api.Modules.Roles.Domain;
using StockMarketSimulator.Api.Modules.Roles.Persistence;

namespace StockMarketSimulator.Api.Modules.Roles;

public static class RolesDependencyInjection
{
    public static IServiceCollection AddRolesModule(this IServiceCollection services)
    {
        services
            .AddPublicApis()
            .AddPersistence();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        return services;
    }

    private static IServiceCollection AddPublicApis(this IServiceCollection services)
    {
        services.AddScoped<IRolesApi, RolesApi>();

        return services;
    }
}
