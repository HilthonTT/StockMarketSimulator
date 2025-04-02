using Microsoft.AspNetCore.SignalR;
using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

public sealed class StocksFeedHub : Hub<IStocksUpdateClient>
{
    public Task JoinGroup(string ticker)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }
}
