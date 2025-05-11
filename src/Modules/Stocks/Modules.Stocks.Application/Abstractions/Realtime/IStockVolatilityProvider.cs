namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IStockVolatilityProvider
{
    (double Mu, double Sigma) GetParameters(string ticker);
}
