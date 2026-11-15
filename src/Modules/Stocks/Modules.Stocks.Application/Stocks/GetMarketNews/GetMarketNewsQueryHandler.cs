using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.GetMarketNews;

internal sealed class GetMarketNewsQueryHandler(IMarketNewsStore newsStore)
    : IQueryHandler<GetMarketNewsQuery, IReadOnlyList<MarketNewsResponse>>
{
    public Task<Result<IReadOnlyList<MarketNewsResponse>>> Handle(GetMarketNewsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(newsStore.GetLatest(request.Limit)));
    }
}
