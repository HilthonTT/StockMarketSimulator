using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Users.Api;

internal sealed class UsersApi : IUsersApi
{
    private readonly IUserContext _userContext;

    public UsersApi(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Guid UserId => _userContext.UserId;
}
