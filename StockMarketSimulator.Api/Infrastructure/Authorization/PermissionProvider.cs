namespace StockMarketSimulator.Api.Infrastructure.Authorization;

internal sealed class PermissionProvider : IPermissionProvider
{
    public Task<HashSet<string>> GetForUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        HashSet<string> permissionsSet = [];

        return Task.FromResult(permissionsSet);
    }
}
