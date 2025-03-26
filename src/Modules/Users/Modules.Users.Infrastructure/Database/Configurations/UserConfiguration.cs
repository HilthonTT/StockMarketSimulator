﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.ValueObjects;

namespace Modules.Users.Infrastructure.Database.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasConversion(x => x.Value, v => Email.Create(v).Value);
        
        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasConversion(x => x.Value, v => Username.Create(v).Value);

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
