using Npgsql;

namespace StockMarketSimulator.Api.Infrastructure.Database;

public interface IDbConnectionFactory
{
    Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
}
