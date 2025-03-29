namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IActiveTickerManager
{
    void AddTicker(string ticker);

    IReadOnlyCollection<string> GetAllTickers();
}
