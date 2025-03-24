using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Transactions.Application.Buy;
using StockMarketSimulator.Api.Modules.Transactions.Application.GetById;
using StockMarketSimulator.Api.Modules.Transactions.Application.GetByUserId;
using StockMarketSimulator.Api.Modules.Transactions.Application.Sell;
using StockMarketSimulator.Api.Modules.Transactions.Contracts;
using StockMarketSimulator.Api.Modules.Transactions.Domain;
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
        services.AddScoped<ICommandHandler<BuyTransactionCommand, Guid>, BuyTransactionCommandHandler>();
        services.AddScoped<ICommandHandler<SellTransactionCommand, Guid>, SellTransactionCommandHandler>();

        services
            .AddScoped<IQueryHandler<GetTransactionByIdQuery, TransactionResponse>, GetTransactionByIdQueryHandler>();
        services
            .AddScoped<IQueryHandler<GetTransactionsByUserIdQuery, List<TransactionResponse>>, GetTransactionByUserIdQueryHandler>();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
