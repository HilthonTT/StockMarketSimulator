namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IActiveTickerManager
{
    bool AddTicker(string ticker);

    IReadOnlyCollection<string> GetAllTickers();
}
