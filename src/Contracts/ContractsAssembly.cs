using System.Reflection;

namespace Contracts;

public static class ContractsAssembly
{
    public static readonly Assembly Instance = typeof(ContractsAssembly).Assembly;
}
