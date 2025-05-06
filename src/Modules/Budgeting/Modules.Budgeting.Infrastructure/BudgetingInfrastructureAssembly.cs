using System.Reflection;

namespace Modules.Budgeting.Infrastructure;

public static class BudgetingInfrastructureAssembly
{
    public static readonly Assembly Instance = typeof(BudgetingInfrastructureAssembly).Assembly;
}
