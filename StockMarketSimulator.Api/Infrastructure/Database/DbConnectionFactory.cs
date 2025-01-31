using Npgsql;

namespace StockMarketSimulator.Api.Infrastructure.Database;

internal sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public DbConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }
}
