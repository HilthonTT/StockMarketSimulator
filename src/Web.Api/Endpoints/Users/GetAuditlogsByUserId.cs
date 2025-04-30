using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Modules.Budgeting.Application.Auditlogs.GetByUserId;
using Modules.Budgeting.Contracts.AuditLogs;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
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
            var query = new GetAuditlogsByUserIdQuery(userId, searchTerm, page, pageSize, logType);

            Result<PagedList<AuditLogResponse>> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .RequireRateLimiting("token")
        .HasPermission(Permission.Read, Permission.Write);
    }
}
