using FluentValidation;
using NetArchTest.Rules;
using StockMarketSimulator.Api;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using Types = NetArchTest.Rules.Types;

namespace Architecture.Tests.Application;

public sealed class ApplicationTests
{
    [Fact]
    public void CommandHandler_ShouldHave_NameEndingWith_CommandHandler()
    {
        TestResult result = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void CommandHandler_Should_NotBePublic()
    {
        TestResult result = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void QueryHandler_ShouldHave_NameEndingWith_QueryHandler()
    {
        TestResult result = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void QueryHandler_Should_NotBePublic()
    {
        TestResult result = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Validator_ShouldHave_NameEndingWith_Validator()
    {
        TestResult result = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Validator_Should_NotBePublic()
    {
        TestResult result = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Validator_Should_BeSealedPublic()
    {
        TestResult result = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}
