using System.Data;

namespace Application.Abstractions.Data;

public interface IDbConnectionFactory
{
    IDbConnection GetOpenConnection();

    Task<IDbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
}
