using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Idempotency;
using Web.Api.Infrastructure;
using Modules.Budgeting.Application.Orders.Place;
using Modules.Budgeting.Contracts.Orders;

namespace Web.Api.Endpoints.Orders;

internal sealed class Place : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders", async (
            PlaceOrderRequest request,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Create(request, GeneralErrors.UnprocessableRequest)
                .Map(r => new PlaceOrderCommand(r.UserId, r.Ticker, r.Side, r.Type, r.Quantity, r.TriggerPrice))
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .HasPermission(Permission.Read, Permission.Write)
        .AddEndpointFilter<IdempotencyFilter>()
        .RequireFeature(FeatureFlags.UseV1BudgetingApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
