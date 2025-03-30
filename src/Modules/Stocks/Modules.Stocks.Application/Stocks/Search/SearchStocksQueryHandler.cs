using Application.Abstractions.Messaging;
using Contracts.Common;
using Modules.Stocks.Application.Abstractions.Data;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Contracts.AlphaVantage;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Entities;
using Modules.Stocks.Domain.Repositories;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.Search;

internal sealed class SearchStocksQueryHandler(
    IStocksClient stocksClient, 
    IDbContext dbContext, 
    IStockSearchResultRepository stockSearchResultRepository,
    IUnitOfWork unitOfWork) 
    : IQueryHandler<SearchStocksQuery, PagedList<StockSearchResponse>>
{
    public async Task<Result<PagedList<StockSearchResponse>>> Handle(
        SearchStocksQuery request, 
        CancellationToken cancellationToken)
    {
        IQueryable<StockSearchResult> searchQuery = dbContext.StockSearchResults.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            searchQuery = searchQuery.Where(s => s.Name.Contains(request.SearchTerm));
        }

        var stockSearchResponsesQuery = searchQuery
            .Select(s => new StockSearchResponse(
                s.Ticker,
                s.Name,
                s.Type,
                s.Region,
                s.MarketOpen,
                s.Timezone,
                s.Currency));

        PagedList<StockSearchResponse> stockSearches = await PagedList<StockSearchResponse>.CreateAsync(
            stockSearchResponsesQuery, 
            request.Page, 
            request.PageSize,
            cancellationToken);

        if (stockSearches.Items.Count != 0) // If there are results, return them
        {
            return stockSearches;
        }

        AlphaVantageSearchData? result = await stocksClient.SearchTickerAsync(request.SearchTerm, cancellationToken);

        if (result is null || result.BestMatches.Count == 0)
        {
            return stockSearches;
        }

        IEnumerable<StockSearchResult> newStockSearchResults = result.BestMatches
            .Select(m => StockSearchResult.Create(
                m.Symbol,
                m.Name,
                m.Type,
                m.Region,
                m.MarketOpen,
                m.MarketClose,
                m.TimeZone,
                m.Currency));

        stockSearchResultRepository.AddRange(newStockSearchResults);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        IEnumerable<StockSearchResponse> newStockSearchResponses = newStockSearchResults
            .Select(s => new StockSearchResponse(
                s.Ticker,
                s.Name,
                s.Type,
                s.Region,
                s.MarketOpen,
                s.Timezone,
                s.Currency));

        return PagedList<StockSearchResponse>.Create(newStockSearchResponses, request.Page, request.PageSize);
    }
}
