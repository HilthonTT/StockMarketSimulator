using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.GetMarketOverview;

internal sealed class GetMarketOverviewQueryHandler(IMarketDataService marketDataService)
    : IQueryHandler<GetMarketOverviewQuery, MarketOverviewResponse>
{
    public async Task<Result<MarketOverviewResponse>> Handle(
        GetMarketOverviewQuery request,
        CancellationToken cancellationToken)
    {
        return await marketDataService.GetOverviewAsync(cancellationToken);
    }
}
