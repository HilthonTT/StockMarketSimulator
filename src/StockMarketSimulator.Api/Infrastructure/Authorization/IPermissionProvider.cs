namespace StockMarketSimulator.Api.Infrastructure.Authorization;

public interface IPermissionProvider
{
    Task<HashSet<string>> GetForUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
