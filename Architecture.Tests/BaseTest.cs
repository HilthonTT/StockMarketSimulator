using ArchUnitNET.Loader;

namespace Architecture.Tests;

public abstract class BaseTest
{
    public static readonly ArchUnitNET.Domain.Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(Program).Assembly)
        .Build();

    // Users Module and public API
    public static readonly IObjectProvider<IType> UsersModule =
        Types().That().ResideInNamespace("Modules.Users", true).As("Users Module");
    public static readonly IObjectProvider<IType> UsersModuleInternal =
        Types().That().ResideInNamespace("Modules.Users", true)
            .And()
            .AreNotPublic()
            .As("Users Module - Internal API");

    // Stocks Module and public API
    public static readonly IObjectProvider<IType> StocksModule =
        Types().That().ResideInNamespace("Modules.Stocks", true).As("Stocks Module");
    public static readonly IObjectProvider<IType> StocksModuleInternal =
        Types().That().ResideInNamespace("Modules.Stocks", true)
            .And()
            .AreNotPublic()
            .As("Stocks Module - Internal API");

    // Transactions Module and public API
    public static readonly IObjectProvider<IType> TransactionsModule =
       Types().That().ResideInNamespace("Modules.Transactions", true).As("Transactions Module");
    public static readonly IObjectProvider<IType> TransactionsModuleInternal =
        Types().That().ResideInNamespace("Modules.Transactions", true)
            .And()
            .AreNotPublic()
            .As("Transactions Module - Internal API");

    // Budgets Module and public API
    public static readonly IObjectProvider<IType> BudgetsModule =
       Types().That().ResideInNamespace("Modules.Budgets", true).As("Budgets Module");
    public static readonly IObjectProvider<IType> BudgetsModuleInternal =
        Types().That().ResideInNamespace("Modules.Budgets", true)
            .And()
            .AreNotPublic()
            .As("Budgets Module - Internal API");

    // Roles Module and public API
    public static readonly IObjectProvider<IType> RolesModule =
       Types().That().ResideInNamespace("Modules.Roles", true).As("Roles Module");
    public static readonly IObjectProvider<IType> RolesModuleInternal =
        Types().That().ResideInNamespace("Modules.Roles", true)
            .And()
            .AreNotPublic()
            .As("Roles Module - Internal API");
}
