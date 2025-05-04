using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Infrastructure.Database;
using Modules.Stocks.Infrastructure.Database;
using Modules.Users.Infrastructure.Database;

namespace Web.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using UsersDbContext usersDbContext =
            scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        usersDbContext.Database.Migrate();

        using StocksDbContext stocksDbContext =
             scope.ServiceProvider.GetRequiredService<StocksDbContext>();

        stocksDbContext.Database.Migrate();

        using BudgetingDbContext budgetingDbContext =
            scope.ServiceProvider.GetRequiredService<BudgetingDbContext>();

        budgetingDbContext.Database.Migrate();

        using GeneralDbContext generalDbContext =
            scope.ServiceProvider.GetRequiredService<GeneralDbContext>();

        generalDbContext.Database.Migrate();
    }
}
