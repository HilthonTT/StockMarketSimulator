namespace StockMarketSimulator.Api.Modules.Users.Infrastructure;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
