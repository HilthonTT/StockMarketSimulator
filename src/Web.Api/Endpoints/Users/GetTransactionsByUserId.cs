using MediatR;
using Microsoft.AspNetCore.Mvc;
using Modules.Budgeting.Application.Transactions.GetByUserId;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetTransactionsByUserId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/transactions", async (
           Guid userId,
           [FromQuery] int page,
           [FromQuery] int pageSize,
           [FromQuery] string? searchTerm,
           [FromQuery] DateTime? StartDate,
           [FromQuery] DateTime? EndDate,
           ISender sender,
           CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetTransactionsByUserIdQuery(userId, page, pageSize, searchTerm, StartDate, EndDate))
                  .Bind(query => sender.Send(query, cancellationToken))
                  .Match(Results.Ok, CustomResults.Problem);
        })
       .WithOpenApi()
       .WithTags(Tags.Users)
       .HasPermission(Permission.Read, Permission.Write)
       .RequireFeature(FeatureFlags.UseV1UsersApi)
       .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
