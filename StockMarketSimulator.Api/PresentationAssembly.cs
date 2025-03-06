using System.Reflection;

namespace StockMarketSimulator.Api;

public static class PresentationAssembly
{
    public static readonly Assembly Instance = typeof(PresentationAssembly).Assembly;
}
