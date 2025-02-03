using SharedKernel;
using StockMarketSimulator.Api.Endpoints;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Modules.Users.Application.Login;
using StockMarketSimulator.Api.Modules.Users.Application.Register;
using StockMarketSimulator.Api.Modules.Users.Presentation.Contracts;

namespace StockMarketSimulator.Api.Modules.Users.Presentation;

internal sealed class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/register", async (
            RegisterUserRequest request,
            RegisterUserCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Username, request.Password);

            Result result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users);

        app.MapPost("/users/login", async (
            LoginUserRequest request,
            LoginUserCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            Result<TokenResponse> result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
