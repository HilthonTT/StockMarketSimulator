﻿using StockMarketSimulator.Api.Modules.Transactions.Domain;
using StockMarketSimulator.Api.Modules.Transactions.Persistence;

namespace StockMarketSimulator.Api.Modules.Transactions;

public static class TransactionDependencyInjection
{
    public static IServiceCollection AddTransactionsModule(this IServiceCollection services)
    {
        services
            .AddPersistence()
            .AddUseCases();

        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
