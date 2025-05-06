using System.Reflection;

namespace Modules.Budgeting.Api;

public static class BudgetingApiAssembly
{
    public static readonly Assembly Instance = typeof(BudgetingApiAssembly).Assembly;
}
