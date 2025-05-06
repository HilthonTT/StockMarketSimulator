using System.Reflection;

namespace Modules.Stocks.Application;

public static class StocksApplicationAssembly
{
    public static readonly Assembly Instance = typeof(StocksApplicationAssembly).Assembly;
}
