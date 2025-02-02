using SharedKernel;
using StockMarketSimulator.Api.Endpoints;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Infrastructure.Events;
using StockMarketSimulator.Api.Modules.Users.Application.Register;

namespace StockMarketSimulator.Api.Modules.Users.Presentation;

internal sealed class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (
            RegisterUserRequest request,
            IEventBus eventBus,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Username, request.Password);

            Result<RegisterUserResponse> result = await eventBus.SendAsync<RegisterUserCommand, RegisterUserResponse>(
                command, 
                cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
