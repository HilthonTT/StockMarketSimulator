using SharedKernel;
using StockMarketSimulator.Api.Endpoints;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Modules.Budgets.Application.GetByUserId;
using StockMarketSimulator.Api.Modules.Budgets.Contracts;

namespace StockMarketSimulator.Api.Modules.Budgets.Presentation;

internal sealed class BudgetEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{userId:guid}/budgets", async (
            Guid userId,
            GetBudgetByUserIdQueryHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBudgetByUserIdQuery(userId);

            Result<BudgetResponse> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Budgets)
        .RequireAuthorization();
    }
}
