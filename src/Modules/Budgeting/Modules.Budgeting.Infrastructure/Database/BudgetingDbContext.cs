﻿using Infrastructure.Database.Configurations;
using Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database;

public sealed class BudgetingDbContext(DbContextOptions<BudgetingDbContext> options)
    : DbContext(options), IUnitOfWork, IDbContext
{
    public DbSet<Budget> Budgets { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetingDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        modelBuilder.HasDefaultSchema(Schemas.Budgeting);

        modelBuilder.ApplyUtcDateTimeConverter();
    }
}
