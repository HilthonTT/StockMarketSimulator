using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Application.Abstractions.Data;

public interface IDbContext
{
    DbSet<Transaction> Transactions { get; }

    DbSet<Budget> Budgets { get; }

    DbSet<AuditLog> AuditLogs { get; }
}
