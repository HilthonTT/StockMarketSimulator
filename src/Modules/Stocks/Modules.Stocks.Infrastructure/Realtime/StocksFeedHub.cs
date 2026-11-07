using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

[Authorize]
public sealed class StocksFeedHub(IActiveTickerManager activeTickerManager) : Hub<IStocksUpdateClient>
{
    public Task JoinGroup(string ticker)
    {
        string normalizedTicker = Normalize(ticker);

        // Subscribing is enough to start receiving simulated updates for a ticker.
        activeTickerManager.AddTicker(normalizedTicker);

        return Groups.AddToGroupAsync(Context.ConnectionId, normalizedTicker);
    }

    public Task LeaveGroup(string ticker)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, Normalize(ticker));
    }

    // Groups and broadcasts use upper-case tickers (see StockService).
    private static string Normalize(string ticker) => ticker.Trim().ToUpperInvariant();
}
