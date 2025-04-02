using Application.Abstractions.Messaging;
using Contracts.Common;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.Search;

public sealed record SearchStocksQuery(string? SearchTerm, int Page, int PageSize) : IQuery<PagedList<StockSearchResponse>>;
