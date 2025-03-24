using Npgsql;
using StockMarketSimulator.Api.Modules.Roles.Domain;

namespace StockMarketSimulator.Api.Modules.Roles.Api;

internal sealed class RolesApi : IRolesApi
{
    private readonly IUserRoleRepository _userRoleRepository;

    public RolesApi(IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public Task CreateUserRoleAsync(
        NpgsqlConnection connection, 
        UserRoleApiResponse userRoleApiResponse, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        var userRole = UserRole.Create(userRoleApiResponse.UserId, userRoleApiResponse.RoleId);

        return _userRoleRepository.CreateAsync(connection, userRole, transaction, cancellationToken);
    }

    public Task<List<string>> GetUserRoleNamesByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        return _userRoleRepository.GetRoleNamesByUserIdAsync(connection, userId, transaction, cancellationToken);
    }
}
