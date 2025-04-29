using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

//[Authorize]
public sealed class StocksFeedHub : Hub<IStocksUpdateClient>
{
    public Task JoinGroup(string ticker)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }

    public Task LeaveGroup(string ticker)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, ticker);
    }
}
