namespace Modules.Stocks.Contracts.Stocks;

public sealed record PricePointResponse(DateTime Time, decimal Open, decimal High, decimal Low, decimal Close);

public sealed record PriceHistoryResponse(string Ticker, string Range, IReadOnlyList<PricePointResponse> Points);
