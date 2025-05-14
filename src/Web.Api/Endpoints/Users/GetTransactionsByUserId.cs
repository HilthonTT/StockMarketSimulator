using Application.Abstractions.Messaging;
using Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using Modules.Budgeting.Application.Transactions.GetByUserId;
using Modules.Budgeting.Contracts.Transactions;
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
           [FromQuery] Guid? cursor,
           [FromQuery] int pageSize,
           [FromQuery] string? searchTerm,
           [FromQuery] DateTime? StartDate,
           [FromQuery] DateTime? EndDate,
           IQueryHandler<GetTransactionsByUserIdQuery, CursorResponse<List<TransactionResponse>>> handler,
           CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetTransactionsByUserIdQuery(userId, cursor, searchTerm, pageSize, StartDate, EndDate))
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
