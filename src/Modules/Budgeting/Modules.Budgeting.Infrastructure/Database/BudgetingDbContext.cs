using Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database;

public sealed class BudgetingDbContext(DbContextOptions<BudgetingDbContext> options)
    : DbContext(options), IUnitOfWork, IDbContext
{
    public DbSet<Budget> Budgets { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetingDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        modelBuilder.HasDefaultSchema(Schemas.Budgeting);
    }
}
