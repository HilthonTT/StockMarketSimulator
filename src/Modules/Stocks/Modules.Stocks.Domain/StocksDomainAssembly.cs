using System.Reflection;

namespace Modules.Stocks.Domain;

public static class StocksDomainAssembly
{
    public static readonly Assembly Instance = typeof(StocksDomainAssembly).Assembly;
}
