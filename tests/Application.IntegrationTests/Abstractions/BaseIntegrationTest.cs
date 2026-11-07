using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Modules.Budgeting.Infrastructure.Database;
using Modules.Stocks.Infrastructure.Database;
using Modules.Users.Infrastructure.Database;

namespace Application.IntegrationTests.Abstractions;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        _scope = factory.Services.CreateScope();
        UsersDbContext = _scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        StocksDbContext = _scope.ServiceProvider.GetRequiredService<StocksDbContext>();
        BudgetingDbContext = _scope.ServiceProvider.GetRequiredService<BudgetingDbContext>();
        Faker = new Faker();
    }

    protected IntegrationTestWebAppFactory Factory { get; }

    /// <summary>The test's DI scope: resolve scoped services (handlers, repositories) from here.</summary>
    protected IServiceProvider ScopedServices => _scope.ServiceProvider;

    protected UsersDbContext UsersDbContext { get; }

    protected StocksDbContext StocksDbContext { get; }

    protected BudgetingDbContext BudgetingDbContext { get; }

    protected Faker Faker { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _scope.Dispose();
    }
}
