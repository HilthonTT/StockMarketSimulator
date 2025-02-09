namespace Architecture.Tests.Modules;

public sealed class StocksModuleBoundariesTests : BaseTest
{
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
    public void StocksModule_ShouldNot_DependOn_TransactionsModule()
    {
        // First, let's verify we're actually finding the types
        IEnumerable<IType> stocksType = StocksModule.GetObjects(Architecture);
        IEnumerable<IType> transactionTypes = TransactionsModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {stocksType.Count()} stock types");
        Debug.WriteLine($"Found {transactionTypes.Count()} transaction types");

        foreach (IType type in stocksType)
        {
            Debug.WriteLine($"Stock type: {type.FullName}");
        }

        // Define and check the rule
        TypesShouldConjunctionWithDescription rule = Types().That().Are(StocksModule)
            .Should().NotDependOnAny(TransactionsModuleInternal)
            .Because("Stocks module should not depend on Transactions module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void StocksModule_ShouldNot_DependOn_BudgetsModule()
    {
        // First, let's verify we're actually finding the types
        IEnumerable<IType> stocksType = StocksModule.GetObjects(Architecture);
        IEnumerable<IType> budgetTypes = BudgetsModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {stocksType.Count()} stock types");
        Debug.WriteLine($"Found {budgetTypes.Count()} budget types");

        foreach (IType type in stocksType)
        {
            Debug.WriteLine($"Stock type: {type.FullName}");
        }

        // Define and check the rule
        TypesShouldConjunctionWithDescription rule = Types().That().Are(StocksModule)
            .Should().NotDependOnAny(BudgetsModuleInternal)
            .Because("Stocks module should not depend on Budgets module internal types");

        rule.Check(Architecture);
    }
}
