using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Idempotency;
using Web.Api.Infrastructure;
using Modules.Budgeting.Application.Budgets.Reset;

namespace Web.Api.Endpoints.Users;

internal sealed class ResetBudget : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId:guid}/budget/reset", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new ResetBudgetCommand(userId))
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Budgets)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireFeature(FeatureFlags.UseV1BudgetingApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
