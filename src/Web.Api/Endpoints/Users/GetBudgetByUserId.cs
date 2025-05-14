using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Budgets.GetByUserId;
using Modules.Budgeting.Contracts.Budgets;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetBudgetByUserId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/budget", async (
            Guid userId,
            IQueryHandler<GetBudgetByUserIdQuery, BudgetResponse> handler,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetBudgetByUserIdQuery(userId))
                .Bind(query => handler.Handle(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }   
}
