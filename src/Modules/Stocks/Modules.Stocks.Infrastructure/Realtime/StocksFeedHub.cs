using Microsoft.AspNetCore.SignalR;
using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

internal sealed class StocksFeedHub : Hub<IStocksUpdateClient>
{
    public Task JoinGroup(string ticker)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }
}
