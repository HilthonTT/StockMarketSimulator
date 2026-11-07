using System.Reflection;
using Application.Abstractions.Events;
using Modules.Budgeting.IntegrationEvents;
using Modules.Users.BackgroundJobs.Users.RevokeExpiredRefreshTokens;
using Modules.Users.IntegrationEvents;
using NetArchTest.Rules;

namespace ArchitectureTests.Modules;

/// <summary>
/// Rules that span modules: integration events live in contract projects, and the Application layers
/// stay free of Quartz, ASP.NET and other modules' internals.
/// </summary>
public sealed class ModuleContractTests
{
    private static readonly Assembly[] IntegrationEventAssemblies =
    [
        UsersIntegrationEventsAssembly.Instance,
        BudgetingIntegrationEventsAssembly.Instance,
    ];

    private static readonly Assembly[] ApplicationAssemblies =
    [
        typeof(global::Modules.Users.Application.Users.GetById.GetUserByIdQuery).Assembly,
        typeof(global::Modules.Stocks.Application.Stocks.GetByTicker.GetStockByTickerQuery).Assembly,
        typeof(global::Modules.Budgeting.Application.Budgets.GetByUserId.GetBudgetByUserIdQuery).Assembly,
    ];

    private static readonly Assembly UsersBackgroundJobsAssembly = typeof(RevokeExpiredRefreshTokenJob).Assembly;

    public static TheoryData<string> IntegrationEventAssemblyNames =>
        [.. IntegrationEventAssemblies.Select(a => a.GetName().Name!)];

    public static TheoryData<string> ApplicationAssemblyNames =>
        [.. ApplicationAssemblies.Select(a => a.GetName().Name!)];

    [Theory]
    [MemberData(nameof(IntegrationEventAssemblyNames))]
    public void IntegrationEvents_Should_BeSealedIntegrationEventTypes(string assemblyName)
    {
        TestResult result = Types.InAssembly(Load(assemblyName))
            .That()
            .AreClasses()
            .And()
            .DoNotHaveNameEndingWith("Assembly")
            .Should()
            .ImplementInterface(typeof(IIntegrationEvent))
            .And()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(IntegrationEventAssemblyNames))]
    public void IntegrationEvents_Should_NotDependOnModuleInternals(string assemblyName)
    {
        TestResult result = Types.InAssembly(Load(assemblyName))
            .ShouldNot()
            .HaveDependencyOnAny(
                "Modules.Users.Domain", "Modules.Users.Application", "Modules.Users.Infrastructure",
                "Modules.Budgeting.Domain", "Modules.Budgeting.Application", "Modules.Budgeting.Infrastructure",
                "Modules.Stocks.Domain", "Modules.Stocks.Application", "Modules.Stocks.Infrastructure",
                "Infrastructure", "Quartz", "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(ApplicationAssemblyNames))]
    public void ApplicationLayer_Should_NotDefineIntegrationEvents(string assemblyName)
    {
        IEnumerable<Type> integrationEvents = Types.InAssembly(Load(assemblyName))
            .That()
            .ImplementInterface(typeof(IIntegrationEvent))
            .GetTypes();

        Assert.Empty(integrationEvents);
    }

    [Theory]
    [MemberData(nameof(ApplicationAssemblyNames))]
    public void ApplicationLayer_Should_NotDependOnQuartzOrAspNetHttp(string assemblyName)
    {
        TestResult result = Types.InAssembly(Load(assemblyName))
            .ShouldNot()
            .HaveDependencyOnAny("Quartz", "Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(ApplicationAssemblyNames))]
    public void ApplicationLayer_Should_NotDependOnBackgroundJobsOrEventsProjects(string assemblyName)
    {
        TestResult result = Types.InAssembly(Load(assemblyName))
            .ShouldNot()
            .HaveDependencyOnAny(
                "Modules.Users.BackgroundJobs", "Modules.Budgeting.BackgroundJobs", "Modules.Stocks.BackgroundJobs",
                "Modules.Users.Events", "Modules.Budgeting.Events")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void UsersBackgroundJobs_Should_NotDependOnBudgetingDomain()
    {
        TestResult result = Types.InAssembly(UsersBackgroundJobsAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Budgeting.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static Assembly Load(string assemblyName) =>
        IntegrationEventAssemblies.Concat(ApplicationAssemblies).Single(a => a.GetName().Name == assemblyName);

    private static string Describe(TestResult result) =>
        result.IsSuccessful ? string.Empty : "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
