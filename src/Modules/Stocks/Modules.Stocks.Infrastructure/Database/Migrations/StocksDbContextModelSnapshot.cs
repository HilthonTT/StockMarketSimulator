﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Modules.Stocks.Infrastructure.Database;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.Stocks.Infrastructure.Database.Migrations
{
    [DbContext(typeof(StocksDbContext))]
    partial class StocksDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("stocks")
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Infrastructure.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on_utc");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.HasKey("Id")
                        .HasName("pk_outbox_messages");

                    b.ToTable("outbox_messages", "stocks");
                });

            modelBuilder.Entity("Modules.Stocks.Domain.Entities.ShortenedUrl", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on_utc");

                    b.Property<DateTime?>("ModifiedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("modified_on_utc");

                    b.Property<string>("OriginalUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("original_url");

                    b.Property<string>("ShortCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("short_code");

                    b.HasKey("Id")
                        .HasName("pk_shortened_urls");

                    b.HasIndex("ShortCode")
                        .IsUnique()
                        .HasDatabaseName("ix_shortened_urls_short_code");

                    b.ToTable("shortened_urls", "stocks");
                });

            modelBuilder.Entity("Modules.Stocks.Domain.Entities.Stock", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on_utc");

                    b.Property<DateTime?>("ModifiedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("modified_on_utc");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric")
                        .HasColumnName("price");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("ticker");

                    b.HasKey("Id")
                        .HasName("pk_stocks");

                    b.HasIndex("CreatedOnUtc")
                        .HasDatabaseName("ix_stocks_created_on_utc");

                    b.HasIndex("Ticker")
                        .HasDatabaseName("ix_stocks_ticker");

                    b.ToTable("stocks", "stocks");
                });

            modelBuilder.Entity("Modules.Stocks.Domain.Entities.StockSearchResult", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on_utc");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency");

                    b.Property<string>("MarketClose")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("market_close");

                    b.Property<string>("MarketOpen")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("market_open");

                    b.Property<DateTime?>("ModifiedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("modified_on_utc");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("region");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("ticker");

                    b.Property<string>("Timezone")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("timezone");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_stock_search_results");

                    b.HasIndex("CreatedOnUtc")
                        .HasDatabaseName("ix_stock_search_results_created_on_utc");

                    b.HasIndex("Ticker")
                        .HasDatabaseName("ix_stock_search_results_ticker");

                    b.HasIndex("Ticker", "Name")
                        .HasDatabaseName("ix_stock_search_results_ticker_name")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Ticker", "Name"), "GIN");

                    b.ToTable("stock_search_results", "stocks");
                });
#pragma warning restore 612, 618
        }
    }
}
