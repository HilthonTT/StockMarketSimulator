namespace Architecture.Tests.Modules;

public sealed class BudgetsModuleBoundariesTests : BaseTest
{
    [Fact]
    public void BudgetsModule_ShouldNot_DependOn_UsersModule()
    {
        IEnumerable<IType> budgetTypes = BudgetsModule.GetObjects(Architecture);
        IEnumerable<IType> userTypes = UsersModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {budgetTypes.Count()} budget types");
        Debug.WriteLine($"Found {userTypes.Count()} user types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(BudgetsModule)
            .Should().NotDependOnAny(UsersModuleInternal)
            .Because("Budgets module should not depend on Users module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void BudgetsModule_ShouldNot_DependOn_TransactionModules()
    {
        IEnumerable<IType> budgetTypes = BudgetsModule.GetObjects(Architecture);
        IEnumerable<IType> transactionTypes = TransactionsModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {budgetTypes.Count()} budget types");
        Debug.WriteLine($"Found {transactionTypes.Count()} transaction types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(BudgetsModule)
            .Should().NotDependOnAny(TransactionsModuleInternal)
            .Because("Budgets module should not depend on Transactions module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void BudgetsModule_ShouldNot_DependOn_StocksModules()
    {
        IEnumerable<IType> budgetTypes = BudgetsModule.GetObjects(Architecture);
        IEnumerable<IType> stockTypes = StocksModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {budgetTypes.Count()} budget types");
        Debug.WriteLine($"Found {stockTypes.Count()} stock types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(BudgetsModule)
            .Should().NotDependOnAny(StocksModuleInternal)
            .Because("Budgets module should not depend on Stocks module internal types");

        rule.Check(Architecture);
    }
}
