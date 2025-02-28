using SharedKernel;
using StockMarketSimulator.Api.Endpoints;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Infrastructure.Ratelimit;
using StockMarketSimulator.Api.Modules.Roles.Domain;
using StockMarketSimulator.Api.Modules.Users.Application.ChangePassword;
using StockMarketSimulator.Api.Modules.Users.Application.DeleteAll;
using StockMarketSimulator.Api.Modules.Users.Application.GetById;
using StockMarketSimulator.Api.Modules.Users.Application.GetCurrent;
using StockMarketSimulator.Api.Modules.Users.Application.Login;
using StockMarketSimulator.Api.Modules.Users.Application.LoginWithRefreshToken;
using StockMarketSimulator.Api.Modules.Users.Application.Register;
using StockMarketSimulator.Api.Modules.Users.Application.RevokeRefreshTokens;
using StockMarketSimulator.Api.Modules.Users.Contracts;

namespace StockMarketSimulator.Api.Modules.Users.Presentation;

internal sealed class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", async (
            GetCurrentUserQueryHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCurrentUserQuery();

            Result<UserResponse> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .RequireAuthorization()
        .WithTags(Tags.Users);

        app.MapGet("/users/{userId:guid}", async (
            Guid userId,
            GetUserByIdQueryHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserByIdQuery(userId);

            Result<UserResponse> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin))
        .WithTags(Tags.Users);

        app.MapPost("/users/register", async (
            RegisterUserRequest request,
            RegisterUserCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Username, request.Password, request.ConfirmPassword);

            Result result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .RequireRateLimiting(RatelimitPolicyNames.SlidingWindow);

        app.MapPost("/users/login", async (
            LoginUserRequest request,
            LoginUserCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            Result<TokenResponse> result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .RequireRateLimiting(RatelimitPolicyNames.SlidingWindow);

        app.MapPost("/users/refresh-token", async (
            LoginWithRefreshTokenRequest request,
            LoginUserWithRefreshTokenCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserWithRefreshTokenCommand(request.RefreshToken);

            Result<TokenResponse> result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .RequireRateLimiting(RatelimitPolicyNames.SlidingWindow);

        app.MapPatch("/users/{userId:guid}/change-password", async (
            Guid userId,
            ChangePasswordRequest request,
            ChangeUserPasswordCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangeUserPasswordCommand(userId, request.CurrentPassword, request.NewPassword);

            Result result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .RequireRateLimiting(RatelimitPolicyNames.SlidingWindow);

        app.MapDelete("/users/{userId:guid}/refresh-tokens", async (
            Guid userId,
            RevokeRefreshTokensCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new RevokeRefreshTokensCommand(userId);

            Result result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .RequireAuthorization()
        .WithTags(Tags.Users);

        app.MapDelete("/users", async (
            DeleteAllUsersCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteAllUsersCommand();

            Result result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users);
    }
}
