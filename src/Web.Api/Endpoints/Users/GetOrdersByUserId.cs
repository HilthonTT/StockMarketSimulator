using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Idempotency;
using Web.Api.Infrastructure;
using Modules.Budgeting.Application.Orders.GetByUserId;

namespace Web.Api.Endpoints.Users;

internal sealed class GetOrdersByUserId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/orders", async (
            Guid userId,
            [FromQuery] string? status,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            bool openOnly = string.Equals(status, "open", StringComparison.OrdinalIgnoreCase);

            return await Result.Success(new GetOrdersByUserIdQuery(userId, openOnly))
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireFeature(FeatureFlags.UseV1BudgetingApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
