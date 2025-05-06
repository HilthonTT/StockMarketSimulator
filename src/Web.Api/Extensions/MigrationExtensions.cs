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

        ApplyMigration<UsersDbContext>(scope);

        ApplyMigration<StocksDbContext>(scope);

        ApplyMigration<BudgetingDbContext>(scope);

        ApplyMigration<GeneralDbContext>(scope);
    }

    private static void ApplyMigration<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }
}
