using System.Data;
using Infrastructure.Database.Configurations;
using Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Database;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<RolePermission> RolePermissions { get; set; }

    public DbSet<Permission> Permissions { get; set; }

    public DbSet<Follower> Followers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        
        modelBuilder.HasDefaultSchema(Schemas.Users);

        modelBuilder.ApplyUtcDateTimeConverter();
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}
