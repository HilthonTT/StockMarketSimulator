using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure;

public interface ITokenProvider
{
    string Create(User user);
}
