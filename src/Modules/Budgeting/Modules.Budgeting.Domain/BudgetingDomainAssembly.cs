using System.Reflection;

namespace Modules.Budgeting.Domain;

public static class BudgetingDomainAssembly
{
    public static readonly Assembly Instance = typeof(BudgetingDomainAssembly).Assembly;
}
