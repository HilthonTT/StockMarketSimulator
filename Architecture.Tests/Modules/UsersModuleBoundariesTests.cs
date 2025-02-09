namespace Architecture.Tests.Modules;

public sealed class UsersModuleBoundariesTests : BaseTest
{
    [Fact]
    public void UsersModule_ShouldNot_DependOn_StocksModule()
    {
        IEnumerable<IType> userTypes = UsersModule.GetObjects(Architecture);
        IEnumerable<IType> stockTypes = StocksModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {userTypes.Count()} user types");
        Debug.WriteLine($"Found {stockTypes.Count()} stock types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(UsersModule)
            .Should().NotDependOnAny(StocksModuleInternal)
            .Because("Users module should not depend on Stocks module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void UsersModule_ShouldNot_DependOn_TransactionsModule()
    {
        IEnumerable<IType> userTypes = UsersModule.GetObjects(Architecture);
        IEnumerable<IType> transactionTypes = TransactionsModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {userTypes.Count()} user types");
        Debug.WriteLine($"Found {transactionTypes.Count()} transaction types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(UsersModule)
            .Should().NotDependOnAny(TransactionsModuleInternal)
            .Because("Users module should not depend on Transactions module internal types");

        rule.Check(Architecture);
    }

    [Fact]
    public void UsersModule_ShouldNot_DependOn_BudgetsModule()
    {
        IEnumerable<IType> userTypes = UsersModule.GetObjects(Architecture);
        IEnumerable<IType> transactionTypes = BudgetsModule.GetObjects(Architecture);

        Debug.WriteLine($"Found {userTypes.Count()} user types");
        Debug.WriteLine($"Found {transactionTypes.Count()} budget types");

        TypesShouldConjunctionWithDescription rule = Types().That().Are(UsersModule)
            .Should().NotDependOnAny(BudgetsModuleInternal)
            .Because("Users module should not depend on Budgets module internal types");

        rule.Check(Architecture);
    }
}
