using System.Reflection;

namespace Modules.Stocks.Api;

public static class StocksApiAssembly
{
    public static readonly Assembly Instance = typeof(StocksApiAssembly).Assembly;
}
