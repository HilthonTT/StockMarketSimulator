using MediatR;
using Microsoft.AspNetCore.Mvc;
using Modules.Budgeting.Application.Auditlogs.GetByUserId;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetAuditlogsByUserId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId:guid}/auditlogs", async (
            Guid userId,
           [FromQuery] int page,
           [FromQuery] int pageSize,
           [FromQuery] string? searchTerm,
           [FromQuery] int? logType,
           ISender sender,
           CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetAuditlogsByUserIdQuery(userId, searchTerm, page, pageSize, logType))
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
