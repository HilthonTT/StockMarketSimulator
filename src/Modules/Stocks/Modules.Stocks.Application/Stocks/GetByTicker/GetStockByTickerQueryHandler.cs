using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Errors;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.GetByTicker;

internal sealed class GetStockByTickerQueryHandler(IStockService stockService) 
    : IQueryHandler<GetStockByTickerQuery, StockQuoteResponse>
{
    public async Task<Result<StockQuoteResponse>> Handle(GetStockByTickerQuery request, CancellationToken cancellationToken)
    {
        Option<StockQuoteResponse> optionQuote = await stockService.GetQuoteAsync(
            request.Ticker,
            trackActivity: true,
            cancellationToken);

        if (!optionQuote.IsSome)
        {
            return Result.Failure<StockQuoteResponse>(StockErrors.NotFound(request.Ticker));
        }

        return optionQuote.ValueOrThrow();
    }
}
