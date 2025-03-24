using Npgsql;
using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure;

internal interface ITokenProvider
{
    Task<string> Create(NpgsqlConnection connection, User user);

    string GenerateRefreshToken();
}
