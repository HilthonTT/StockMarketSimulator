using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Errors;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.GetByTicker;

internal sealed class GetStockByTickerQueryHandler(IStockService stockService) 
    : IQueryHandler<GetStockByTickerQuery, StockPriceResponse>
{
    public async Task<Result<StockPriceResponse>> Handle(GetStockByTickerQuery request, CancellationToken cancellationToken)
    {
        StockPriceResponse? result = await stockService.GetLatestStockPriceAsync(request.Ticker, cancellationToken);

        if (result is null)
        {
            return Result.Failure<StockPriceResponse>(StockErrors.NotFound(request.Ticker));
        }

        return result;
    }
}
