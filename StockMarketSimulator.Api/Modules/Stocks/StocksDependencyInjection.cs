using SharedKernel;
using StockMarketSimulator.Api.Modules.Stocks.Api;
using StockMarketSimulator.Api.Modules.Stocks.Application.GetByTicker;
using StockMarketSimulator.Api.Modules.Stocks.Application.Search;
using StockMarketSimulator.Api.Modules.Stocks.Domain;
using StockMarketSimulator.Api.Modules.Stocks.Infrastructure;
using StockMarketSimulator.Api.Modules.Stocks.Persistence;

namespace StockMarketSimulator.Api.Modules.Stocks;

public static class StocksDependencyInjection
{
    public static IServiceCollection AddStocksModule(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddUseCases()
            .AddPersistence()
            .AddRealtimeStock(configuration)
            .AddPublicApis();

        return services;
    }

    private static IServiceCollection AddPublicApis(this IServiceCollection services)
    {
        services.AddScoped<IStocksApi, StocksApi>();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IStockRepository, StockRepository>();

        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<GetStockTickerQueryHandler>();
        services.AddScoped<SearchStocksQueryHandler>();

        return services;
    }

    private static IServiceCollection AddRealtimeStock(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IStockService, StockService>();
        services.AddSingleton<ActiveTickerManager>();

        services.AddHttpClient<StocksClient>(httpClient =>
        {
            string? apiUrl = configuration["Stocks:ApiUrl"];
            Ensure.NotNullOrEmpty(apiUrl, nameof(apiUrl));

            httpClient.BaseAddress = new Uri(apiUrl);
        })
        .AddStandardResilienceHandler();

        return services;
    }
}
