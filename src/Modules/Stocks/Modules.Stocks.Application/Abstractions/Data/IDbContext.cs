using System.Data;
using Microsoft.EntityFrameworkCore;
using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Application.Abstractions.Data;

public interface IDbContext
{
    DbSet<Stock> Stocks { get; }

    DbSet<StockSearchResult> StockSearchResults { get; }

    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
