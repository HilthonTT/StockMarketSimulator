﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Infrastructure.Database.Configurations;

internal sealed class StockSearchResultConfiguration : IEntityTypeConfiguration<StockSearchResult>
{
    public void Configure(EntityTypeBuilder<StockSearchResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Ticker).HasMaxLength(10);

        builder.HasIndex(x => x.Ticker);
        builder.HasIndex(x => x.CreatedOnUtc);

        builder.HasIndex(x => new { x.Ticker, x.Name })
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");
    }
}
