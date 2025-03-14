using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using WireMock.Server;

namespace Api.Tests.Infrastructure;

public sealed class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
       .WithImage("postgres:17.2")
       .WithDatabase("stockmarketsimulator")
       .WithUsername("postgres")
       .WithPassword("postgres")
       .WithCleanUp(true)
       .WithAutoRemove(true)
       .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .WithCleanUp(true)
        .WithAutoRemove(true)
        .Build();

    public WireMockServer WireMockServer { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
        builder.UseSetting("Stocks:ApiKey", "WI9KM68A1KNQNRBK");
        builder.UseSetting("Stocks:ApiUrl", "https://www.alphavantage.co/query");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();

        WireMockServer = WireMockServer.Start();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        WireMockServer?.Dispose();

        await base.DisposeAsync();
    }
}
