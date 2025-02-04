using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure;

internal interface ITokenProvider
{
    string Create(User user);

    string GenerateRefreshToken();
}
