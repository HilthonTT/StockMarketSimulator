using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

[Authorize]
public sealed class StocksFeedHub(IActiveTickerManager activeTickerManager) : Hub<IStocksUpdateClient>
{
    public async Task JoinGroup(string ticker)
    {
        string normalizedTicker = Normalize(ticker);

        await Groups.AddToGroupAsync(Context.ConnectionId, normalizedTicker);

        // Subscribing is enough to start receiving simulated updates for a ticker. Subscriptions are
        // reference-counted per connection, so the ticker stops being simulated once nobody listens.
        activeTickerManager.Subscribe(Context.ConnectionId, normalizedTicker);
    }

    public async Task LeaveGroup(string ticker)
    {
        string normalizedTicker = Normalize(ticker);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, normalizedTicker);

        activeTickerManager.Unsubscribe(Context.ConnectionId, normalizedTicker);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // SignalR drops the connection's group memberships itself; release its subscriptions too.
        activeTickerManager.RemoveConnection(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }

    // Groups and broadcasts use upper-case tickers (see StockService).
    private static string Normalize(string ticker) => ticker.Trim().ToUpperInvariant();
}
