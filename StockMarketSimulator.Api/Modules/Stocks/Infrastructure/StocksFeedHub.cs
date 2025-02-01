using Microsoft.AspNetCore.SignalR;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class StocksFeedHub : Hub<IStocksUpdateClient>
{
    public async Task JoinGroup(string ticker)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }
}
