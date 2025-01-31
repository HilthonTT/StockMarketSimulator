using ArchUnitNET.Domain;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using System.Diagnostics;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

public class ModuleBoundariesTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(Program).Assembly)
        .Build();

    // Users module and public API
    private static readonly IObjectProvider<IType> UsersModule =
        Types().That().ResideInNamespace("Modules.Users", true).As("Users Module");
    private static readonly IObjectProvider<IType> UsersModuleInternal =
        Types().That().ResideInNamespace("Modules.Users", true)
            .And()
            .AreNotPublic()
            .As("Users Module - Internal API");

    // Stocks module and public API
    private static readonly IObjectProvider<IType> StocksModule =
        Types().That().ResideInNamespace("Modules.Stocks", true).As("Stocks Module");
    private static readonly IObjectProvider<IType> StocksModuleInternal =
        Types().That().ResideInNamespace("Modules.Stocks", true)
            .And()
            .AreNotPublic()
            .As("Stocks Module - Internal API");

    [Fact]
    public void StocksModule_ShouldNot_DependOn_UsersModule()
    {
        // First, let's verify we're actually finding the types
        IEnumerable<IType> stocksType = StocksModule.GetObjects(Architecture);
        IEnumerable<IType> userTypes = UsersModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {stocksType.Count()} stock types");
        Debug.WriteLine($"Found {userTypes.Count()} user types");

        foreach (IType type in stocksType)
        {
            Debug.WriteLine($"Stock type: {type.FullName}");
        }

        // Define and check the rule
        TypesShouldConjunctionWithDescription rule = Types().That().Are(StocksModule)
            .Should().NotDependOnAny(UsersModuleInternal)
            .Because("Stocks module should not depend on Users module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void UsersModule_ShouldNot_DependOn_StocksModule()
    {
        // First, let's verify we're actually finding the types
        IEnumerable<IType> stocksType = StocksModule.GetObjects(Architecture);
        IEnumerable<IType> userTypes = UsersModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {stocksType.Count()} stock types");
        Debug.WriteLine($"Found {userTypes.Count()} user types");

        foreach (IType type in userTypes)
        {
            Debug.WriteLine($"User type: {type.FullName}");
        }

        // Define and check the rule
       TypesShouldConjunctionWithDescription rule = Types().That().Are(UsersModule)
            .Should().NotDependOnAny(StocksModuleInternal)
            .Because("Users module should not depend on Stocks module internal types");

        rule.Check(Architecture);
    }
}
