using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.AlphaVantage;

namespace Modules.Stocks.Application.Stocks.Search;

public sealed record SearchStocksQuery(string SearchTerm) : IQuery<AlphaVantageSearchData>;
