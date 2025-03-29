using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Contracts.AlphaVantage;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.Search;

internal sealed class SearchStocksQueryHandler(IStocksClient stocksClient)
    : IQueryHandler<SearchStocksQuery, AlphaVantageSearchData>
{
    public async Task<Result<AlphaVantageSearchData>> Handle(SearchStocksQuery request, CancellationToken cancellationToken)
    {
        AlphaVantageSearchData? result = await stocksClient.SearchTickerAsync(request.SearchTerm, cancellationToken);

        return result;
    }
}
