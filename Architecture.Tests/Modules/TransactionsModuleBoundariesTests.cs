namespace Architecture.Tests.Modules;

public sealed class TransactionsModuleBoundariesTests : BaseTest
{
    [Fact]
    public void TransactionsModule_ShouldNot_DependOn_UsersModule()
    {
        IEnumerable<IType> transactionTypes = TransactionsModule.GetObjects(Architecture);
        IEnumerable<IType> userTypes = UsersModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {transactionTypes.Count()} transaction types");
        Debug.WriteLine($"Found {userTypes.Count()} user types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(TransactionsModule)
            .Should().NotDependOnAny(UsersModuleInternal)
            .Because("Transactions module should not depend on Users module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void TransactionsModule_ShouldNot_DependOn_BudgetsModule()
    {
        IEnumerable<IType> transactionTypes = TransactionsModule.GetObjects(Architecture);
        IEnumerable<IType> budgetTypes = BudgetsModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {transactionTypes.Count()} transaction types");
        Debug.WriteLine($"Found {budgetTypes.Count()} budget types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(TransactionsModule)
            .Should().NotDependOnAny(BudgetsModuleInternal)
            .Because("Transactions module should not depend on Budgets module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void TransactionsModule_ShouldNot_DependOn_StocksModule()
    {
        IEnumerable<IType> transactionTypes = TransactionsModule.GetObjects(Architecture);
        IEnumerable<IType> stockTypes = StocksModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {transactionTypes.Count()} stock types");
        Debug.WriteLine($"Found {stockTypes.Count()} budget types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(TransactionsModule)
            .Should().NotDependOnAny(StocksModuleInternal)
            .Because("Transactions module should not depend on Stocks module internal types");

        rule.Check(Architecture);
    }
}
