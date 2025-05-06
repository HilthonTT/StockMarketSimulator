using System.Reflection;

namespace Modules.Budgeting.Application;

public static class BudgetingApplicationAssembly
{
    public static readonly Assembly Instance = typeof(BudgetingApplicationAssembly).Assembly;
}
