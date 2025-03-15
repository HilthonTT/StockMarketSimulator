using Dapper;
using Microsoft.Extensions.DependencyInjection;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using StockMarketSimulator.Api.Modules.Users.Contracts;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Api.Tests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    protected readonly CustomWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IDbConnectionFactory DbConnectionFactory;

    protected BaseIntegrationTest(CustomWebAppFactory factory)
    {
        using var scope = factory.Services.CreateScope();

        Factory = factory;
        Client = factory.CreateClient();

        DbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await EnsureDatabaseInitializedAsync();
    }

    protected void SetupStockApiMock(string ticker, StockPriceResponse response)
    {
        Factory.WireMockServer
            .Given(Request.Create()
                .WithPath($"api/v1/stocks/{ticker}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(response));
    }

    internal void SetupUsersApiMock(Guid userId, UserResponse response)
    {
        Factory.WireMockServer
            .Given(Request.Create()
                .WithPath($"api/v1/users/{userId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(response));
    }

    private async Task EnsureDatabaseInitializedAsync()
    {
        const string sql =
            """
            -- Check if users table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.users (
                id UUID PRIMARY KEY,
                email VARCHAR(512) NOT NULL UNIQUE,
                user_name VARCHAR(256) NOT NULL,
                password_hash TEXT NOT NULL
            );

            -- Check if refresh tokens table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.refresh_tokens (
                id UUID PRIMARY KEY,
                token VARCHAR(200) NOT NULL UNIQUE,
                user_id UUID NOT NULL,
                expires_on_utc TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),

                CONSTRAINT fk_refresh_tokens_users FOREIGN KEY (user_id) 
                REFERENCES public.users (id) ON DELETE CASCADE
            );

            -- Check if stock prices table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.stock_prices (
                id UUID PRIMARY KEY,
                ticker VARCHAR(10) NOT NULL,
                price NUMERIC(12, 6) NOT NULL,
                timestamp TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC')
            );

            -- Create an index on the ticker column for faster lookups
            CREATE INDEX IF NOT EXISTS idx_stock_prices_ticker ON public.stock_prices(ticker);
            
            -- Create an index on the timestamp column for faster time-based queries
            CREATE INDEX IF NOT EXISTS idx_stock_prices_timestamp ON public.stock_prices(timestamp);

            -- Check if transactions table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.transactions (
                id UUID PRIMARY KEY,
                user_id UUID NOT NULL,
                ticker VARCHAR(10) NOT NULL,
                limit_price DECIMAL(18, 2) NOT NULL,
                transaction_type INT NOT NULL CHECK (transaction_type IN (0, 1)),
                quantity INT NOT NULL CHECK (quantity > 0),
                created_on_utc TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),

                CONSTRAINT fk_transactions_users FOREIGN KEY (user_id)
                REFERENCES public.users (id) ON DELETE CASCADE
            );

            -- Create an index on the ticker column for faster lookups
            CREATE INDEX IF NOT EXISTS idx_transactions_ticker ON public.transactions(ticker);

            -- Check if budgets table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.budgets (
                id UUID PRIMARY KEY,
                user_id UUID NOT NULL,
                buying_power DECIMAL(18, 2) NOT NULL,

                CONSTRAINT fk_transactions_users FOREIGN KEY (user_id)
                REFERENCES public.users (id) ON DELETE CASCADE
            );
            """;

        await using var connection = await DbConnectionFactory.GetOpenConnectionAsync();

        await connection.ExecuteAsync(sql);
    }
}
