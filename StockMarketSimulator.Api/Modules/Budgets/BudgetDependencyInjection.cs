using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Budgets.Api;
using StockMarketSimulator.Api.Modules.Budgets.Application.GetByUserId;
using StockMarketSimulator.Api.Modules.Budgets.Contracts;
using StockMarketSimulator.Api.Modules.Budgets.Domain;
using StockMarketSimulator.Api.Modules.Budgets.Persistence;

namespace StockMarketSimulator.Api.Modules.Budgets;

public static class BudgetDependencyInjection 
{
    public static IServiceCollection AddBudgetModule(this IServiceCollection services)
    {
        services
            .AddPersistence()
            .AddUseCases()
            .AddPublicApis();

        return services;
    }

    private static IServiceCollection AddPublicApis(this IServiceCollection services)
    {
        services.AddScoped<IBudgetsApi, BudgetsApi>();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IBudgetRepository, BudgetRepository>();

        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetBudgetByUserIdQuery, BudgetResponse>, GetBudgetByUserIdQueryHandler>();

        return services;
    }
}
