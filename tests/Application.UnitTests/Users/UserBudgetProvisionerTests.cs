using EntityFramework.Exceptions.Common;
using Modules.Budgeting.Api.Api;
using Modules.Budgeting.Api.Responses;
using Modules.Users.BackgroundJobs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Application.UnitTests.Users;

public sealed class UserBudgetProvisionerTests
{
    private readonly IBudgetingApi _budgetingApi = Substitute.For<IBudgetingApi>();
    private readonly Guid _userId = Guid.CreateVersion7();

    [Fact]
    public async Task EnsureBudget_Should_CreateBudget_WhenUserHasNone()
    {
        _budgetingApi.GetBudgetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns((BudgetApiResponse?)null);

        await new UserBudgetProvisioner(_budgetingApi).EnsureBudgetAsync(_userId);

        await _budgetingApi.Received(1).AddBudgetAsync(_userId, BudgetCurrencies.Default, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureBudget_Should_DoNothing_WhenBudgetExists()
    {
        _budgetingApi.GetBudgetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new BudgetApiResponse(Guid.CreateVersion7(), _userId, 5000m, BudgetCurrencies.Default));

        await new UserBudgetProvisioner(_budgetingApi).EnsureBudgetAsync(_userId);

        await _budgetingApi.DidNotReceiveWithAnyArgs().AddBudgetAsync(default, default!, default);
    }

    [Fact]
    public async Task EnsureBudget_Should_TreatAConcurrentInsertAsSuccess()
    {
        _budgetingApi.GetBudgetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns((BudgetApiResponse?)null);
        _budgetingApi.AddBudgetAsync(_userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new UniqueConstraintException());

        Exception? exception = await Record.ExceptionAsync(
            () => new UserBudgetProvisioner(_budgetingApi).EnsureBudgetAsync(_userId));

        Assert.Null(exception);
    }
}
