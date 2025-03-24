using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Domain;

namespace StockMarketSimulator.Api.Modules.Stocks.Application.Search;

public sealed record SearchStocksQuery(string SearchTerm) : IQuery<List<Match>>;
