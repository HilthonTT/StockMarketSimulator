using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Idempotency;
using Web.Api.Infrastructure;
using Modules.Budgeting.Application.Orders.Cancel;

namespace Web.Api.Endpoints.Orders;

internal sealed class Cancel : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("orders/{orderId:guid}", async (
            Guid orderId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new CancelOrderCommand(orderId))
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireFeature(FeatureFlags.UseV1BudgetingApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
