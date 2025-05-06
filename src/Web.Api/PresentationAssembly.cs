using System.Reflection;

namespace Web.Api;

public static class PresentationAssembly
{
    public static readonly Assembly Instance = typeof(PresentationAssembly).Assembly;
}
