using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Api.FunctionalTests.Abstractions;

public sealed class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
       .WithImage("postgres:latest")
       .WithDatabase("runtrackr")
       .WithUsername("postgres")
       .WithPassword("postgres")
       .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
       .WithImage(RabbitMqBuilder.RabbitMqImage)
       .Build();

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage(AzuriteBuilder.AzuriteImage)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:stockmarketsimulator-db", _dbContainer.GetConnectionString());
        builder.UseSetting("ConnectionStrings:stockmarketsimulator-redis", _redisContainer.GetConnectionString());
        builder.UseSetting("ConnectionStrings:stockmarketsimulator-blobs", _azuriteContainer.GetConnectionString());
        builder.UseSetting("ConnectionStrings:stockmarketsimulator-rabbitmq", _rabbitMqContainer.GetConnectionString());

        builder.UseSetting("Jwt:Secret", "super-duper-secret-value-that-should-be-in-user-secrets");
        builder.UseSetting("Jwt:Issuer", "stock-market-simulator");
        builder.UseSetting("Jwt:Audience", "investors");
        builder.UseSetting("Jwt:ExpirationInMinutes", "60");

        builder.UseSetting("Stocks:ApiUrl", "https://www.alphavantage.co/query");
        builder.UseSetting("Stocks:ApiKey", "your-api-key-for-tests");

        builder.UseSetting("Email:SenderDisplayName", "Test Sender");
        builder.UseSetting("Email:SenderEmail", "test@ethereal.email");
        builder.UseSetting("Email:SmtpPassword", "your-smtp-password");
        builder.UseSetting("Email:SmtpServer", "smtp.ethereal.email");
        builder.UseSetting("Email:SmtpPort", "587");

        builder.UseSetting("StockUpdateOptions:UpdateIntervalInSeconds", "1");
        builder.UseSetting("StockUpdateOptions:MaxPercentageChange", "0.002");

        builder.UseSetting("Cors:AllowedOrigin", "http://localhost:5173");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _azuriteContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _azuriteContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }
}
