using System.Security.Claims;
using Application.Abstractions.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Web.Api.Idempotency;

namespace Application.UnitTests.Web;

public sealed class IdempotencyFilterTests
{
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly Guid _key = Guid.CreateVersion7();

    [Fact]
    public async Task Should_Return409_AndNotExecute_WhileTheSameKeyIsInFlight()
    {
        _cache.TryClaimAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(false);
        int executions = 0;

        object? result = await new IdempotencyFilter().InvokeAsync(CreateContext(Guid.CreateVersion7()), _ =>
        {
            executions++;
            return ValueTask.FromResult<object?>(TypedResults.Ok(1));
        });

        Assert.Equal(0, executions);
        Assert.Equal(StatusCodes.Status409Conflict, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Fact]
    public async Task Should_StoreSuccessfulResponse_AndKeepTheClaim()
    {
        _cache.TryClaimAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(true);

        await new IdempotencyFilter().InvokeAsync(
            CreateContext(Guid.CreateVersion7()),
            _ => ValueTask.FromResult<object?>(TypedResults.Ok(42)));

        await _cache.Received(1).SetAsync(
            Arg.Any<string>(), Arg.Any<IdempotentResponse>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
        await _cache.DidNotReceiveWithAnyArgs().ReleaseClaimAsync(default!, default);
    }

    [Fact]
    public async Task Should_ReleaseTheClaim_WhenTheRequestFails()
    {
        _cache.TryClaimAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(true);

        await new IdempotencyFilter().InvokeAsync(
            CreateContext(Guid.CreateVersion7()),
            _ => ValueTask.FromResult<object?>(TypedResults.Problem(statusCode: 409)));

        await _cache.Received(1).ReleaseClaimAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CacheKey_Should_BeScopedToTheCaller()
    {
        string first = IdempotencyFilter.CreateCacheKey(CreateContext(Guid.CreateVersion7()).HttpContext, _key);
        string second = IdempotencyFilter.CreateCacheKey(CreateContext(Guid.CreateVersion7()).HttpContext, _key);

        Assert.NotEqual(first, second);
    }

    private DefaultEndpointFilterInvocationContext CreateContext(Guid userId)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddSingleton(_cache).BuildServiceProvider(),
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                authenticationType: "Bearer"))
        };

        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/v1/transactions/buy";
        httpContext.Request.Headers[IdempotencyFilter.HeaderName] = _key.ToString();

        return new DefaultEndpointFilterInvocationContext(httpContext);
    }
}
