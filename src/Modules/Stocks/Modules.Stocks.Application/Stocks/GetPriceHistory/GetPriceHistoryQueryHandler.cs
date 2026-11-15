using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Errors;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.GetPriceHistory;

internal sealed class GetPriceHistoryQueryHandler(
    IStockService stockService,
    IMarketDataService marketDataService) : IQueryHandler<GetPriceHistoryQuery, PriceHistoryResponse>
{
    public async Task<Result<PriceHistoryResponse>> Handle(GetPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        // Lists the ticker (and back-fills its history) on first use, like a quote request.
        Option<StockQuoteResponse> quote = await stockService.GetQuoteAsync(request.Ticker, trackActivity: true, cancellationToken);
        if (!quote.IsSome)
        {
            return Result.Failure<PriceHistoryResponse>(StockErrors.NotFound(request.Ticker));
        }

        PriceRange range = PriceRange.FromName(request.Range) ?? PriceRange.OneDay;

        return await marketDataService.GetHistoryAsync(quote.ValueOrThrow().Ticker, range, cancellationToken);
    }
}
